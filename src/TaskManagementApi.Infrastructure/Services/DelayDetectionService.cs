using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Infrastructure.Services
{
    public class DelayDetectionService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DelayDetectionService> _logger;

        public DelayDetectionService(IServiceProvider serviceProvider, ILogger<DelayDetectionService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Running delay detection job...");
                using (var scope = _serviceProvider.CreateScope())
                {
                    var ticketRepository = scope.ServiceProvider.GetRequiredService<IRepository<Ticket>>();
                    var delayRepository = scope.ServiceProvider.GetRequiredService<IRepository<DelayRecord>>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    var now = DateTime.UtcNow;

                    // 1. Past ExpectedDueDate not terminal
                    var overdueTickets = await ticketRepository.Query()
                        .Include(t => t.Status)
                        .Where(t => t.ExpectedDueDate < now && (t.Status == null || !t.Status.IsTerminal))
                        .ToListAsync();

                    foreach (var ticket in overdueTickets)
                    {
                        var exists = await delayRepository.Query().AnyAsync(d => d.TicketId == ticket.Id && d.DelayType == DelayType.Overdue && d.ResolvedAt == null);
                        if (!exists)
                        {
                            await delayRepository.AddAsync(new DelayRecord
                            {
                                TicketId = ticket.Id,
                                DelayType = DelayType.Overdue,
                                DetectedAt = now,
                                DaysDelayed = (now - ticket.ExpectedDueDate!.Value).Days,
                                Reason = "Automatically detected: Past due date"
                            });

                            await notificationService.SendAsync(new NotificationEvent
                            {
                                Type = NotificationEventType.TicketOverdue,
                                ProjectId = ticket.ProjectId,
                                ReferenceId = ticket.Id,
                                ReferenceType = "Ticket",
                                Title = "Ticket Overdue",
                                Body = $"Ticket {ticket.TicketNumber} is past its expected due date."
                            });
                        }
                    }

                    // Other detection logic...
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
