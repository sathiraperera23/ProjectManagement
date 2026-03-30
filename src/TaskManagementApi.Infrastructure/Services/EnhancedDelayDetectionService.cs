using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Domain.Enums;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace TaskManagementApi.Infrastructure.Services
{
    public class EnhancedDelayDetectionService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EnhancedDelayDetectionService> _logger;

        public EnhancedDelayDetectionService(IServiceProvider serviceProvider, ILogger<EnhancedDelayDetectionService> _logger)
        {
            _serviceProvider = serviceProvider;
            this._logger = _logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Running enhanced delay detection job...");
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var ticketRepository = scope.ServiceProvider.GetRequiredService<IRepository<Ticket>>();
                        var delayRepository = scope.ServiceProvider.GetRequiredService<IRepository<DelayRecord>>();
                        var escalationRepository = scope.ServiceProvider.GetRequiredService<IRepository<EscalationRule>>();
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                        var sprintRepository = scope.ServiceProvider.GetRequiredService<IRepository<Sprint>>();

                        var now = DateTime.UtcNow;

                        var activeTickets = await ticketRepository.Query()
                            .Include(t => t.Status)
                            .Include(t => t.IncomingLinks)
                            .Include(t => t.StatusHistory)
                            .Where(t => t.Status == null || !t.Status.IsTerminal)
                            .ToListAsync();

                        foreach (var ticket in activeTickets)
                        {
                            // 1. Overdue Detection
                            if (ticket.ExpectedDueDate < now)
                            {
                                await HandleDelayAsync(ticket, DelayType.Overdue, now, delayRepository, escalationRepository, notificationService);
                            }

                            // 2. NotStartedLate Detection
                            if (ticket.StartDate < now && (ticket.Status?.Name == "Open" || ticket.Status?.Name == "NotStarted"))
                            {
                                await HandleDelayAsync(ticket, DelayType.NotStartedLate, now, delayRepository, escalationRepository, notificationService);
                            }

                            // 3. PausedExtended Detection (3 business days)
                            if (ticket.Status?.Name == "Paused")
                            {
                                var lastStatusChange = ticket.StatusHistory.OrderByDescending(h => h.ChangedAt).FirstOrDefault();
                                if (lastStatusChange != null && (now - lastStatusChange.ChangedAt).TotalDays > 3)
                                {
                                    await HandleDelayAsync(ticket, DelayType.PausedExtended, now, delayRepository, escalationRepository, notificationService);
                                }
                            }

                            // 4. BlockedUnresolved (> 2 business days)
                            var blockLink = ticket.IncomingLinks.FirstOrDefault(l => l.LinkType == TicketLinkType.Blocks);
                            if (blockLink != null && (now - blockLink.CreatedAt).TotalDays > 2)
                            {
                                await HandleDelayAsync(ticket, DelayType.BlockedUnresolved, now, delayRepository, escalationRepository, notificationService);
                            }

                            // 5. SprintOverrun Risk
                            if (ticket.SprintId.HasValue)
                            {
                                var sprint = await sprintRepository.GetByIdAsync(ticket.SprintId.Value);
                                if (sprint != null && sprint.Status == SprintStatus.Active && sprint.EndDate < now)
                                {
                                    await HandleDelayAsync(ticket, DelayType.SprintOverrun, now, delayRepository, escalationRepository, notificationService);
                                }
                            }
                        }

                        // Resolve records for terminal tickets
                        var terminalTickets = await ticketRepository.Query().Include(t => t.Status).Where(t => t.Status != null && t.Status.IsTerminal).Select(t => t.Id).ToListAsync();
                        var openDelays = await delayRepository.Query().Where(d => terminalTickets.Contains(d.TicketId) && d.ResolvedAt == null).ToListAsync();
                        foreach (var d in openDelays)
                        {
                            d.ResolvedAt = now;
                            await delayRepository.UpdateAsync(d);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in delay detection background task");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task HandleDelayAsync(Ticket ticket, DelayType type, DateTime now, IRepository<DelayRecord> repo, IRepository<EscalationRule> escRepo, INotificationService notif)
        {
            var existing = await repo.Query().FirstOrDefaultAsync(d => d.TicketId == ticket.Id && d.DelayType == type && d.ResolvedAt == null);

            if (existing == null)
            {
                var record = new DelayRecord
                {
                    TicketId = ticket.Id,
                    DelayType = type,
                    DetectedAt = now,
                    DaysDelayed = 0,
                    EscalationLevel = 1
                };
                await repo.AddAsync(record);

                await notif.SendAsync(new NotificationEvent
                {
                    Type = NotificationEventType.TicketOverdue,
                    ProjectId = ticket.ProjectId,
                    ReferenceId = ticket.Id,
                    Title = $"{type} Delay Detected",
                    Body = $"Ticket {ticket.TicketNumber} has been flagged for {type}."
                });
            }
            else
            {
                var days = (now - existing.DetectedAt).Days;
                existing.DaysDelayed = days;

                // Skip escalation level progression if RevisedDueDate is set on the RECORD
                if (existing.RevisedDueDate.HasValue)
                {
                    await repo.UpdateAsync(existing);
                    return;
                }

                var rule = await escRepo.Query().FirstOrDefaultAsync(r => r.ProjectId == ticket.ProjectId)
                           ?? new EscalationRule { EscalateAfterDays = 3, SecondLevelAfterDays = 7, RepeatEveryDays = 3 };

                if (days >= rule.SecondLevelAfterDays && existing.EscalationLevel < 2)
                {
                    existing.EscalationLevel = 2;
                    await notif.SendAsync(new NotificationEvent
                    {
                        Type = NotificationEventType.TicketOverdue,
                        ProjectId = ticket.ProjectId,
                        ReferenceId = ticket.Id,
                        Title = "Director Level Escalation",
                        Body = $"Ticket {ticket.TicketNumber} remains unresolved after {days} days of delay."
                    });
                }

                await repo.UpdateAsync(existing);
            }
        }
    }
}
