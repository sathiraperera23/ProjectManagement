using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Application.DTOs.Reports;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IRepository<BacklogItem> _backlogRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<TicketLink> _ticketLinkRepository;
        private readonly IRepository<TimeLog> _timeLogRepository;
        private readonly IRepository<UserRate> _userRateRepository;
        private readonly IRepository<ProjectBudget> _budgetRepository;
        private readonly IRepository<DelayRecord> _delayRepository;
        private readonly IRepository<Sprint> _sprintRepository;

        public ReportService(
            IRepository<BacklogItem> backlogRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<TicketLink> ticketLinkRepository,
            IRepository<TimeLog> timeLogRepository,
            IRepository<UserRate> userRateRepository,
            IRepository<ProjectBudget> budgetRepository,
            IRepository<DelayRecord> delayRepository,
            IRepository<Sprint> sprintRepository)
        {
            _backlogRepository = backlogRepository;
            _ticketRepository = ticketRepository;
            _ticketLinkRepository = ticketLinkRepository;
            _timeLogRepository = timeLogRepository;
            _userRateRepository = userRateRepository;
            _budgetRepository = budgetRepository;
            _delayRepository = delayRepository;
            _sprintRepository = sprintRepository;
        }

        public async Task<IEnumerable<RtmReportItemDto>> GetRtmReportAsync(int projectId, int? subProjectId = null, int? productId = null)
        {
            var query = _backlogRepository.Query()
                .Include(b => b.TicketLinks).ThenInclude(tl => tl.Ticket).ThenInclude(t => t.Status)
                .Where(b => b.ProjectId == projectId);

            if (subProjectId.HasValue) query = query.Where(b => b.ProjectId == projectId); // Simple logic for scaffold
            if (productId.HasValue) query = query.Where(b => b.ProductId == productId);

            var items = await query.ToListAsync();
            return items.Select(b => new RtmReportItemDto
            {
                RequirementId = $"BRD-{b.Id}",
                RequirementDescription = b.Title,
                UseCaseNumber = b.UseCaseNumber, // Assuming added to BacklogItem
                UserStory = b.Description,
                LinkedTickets = b.TicketLinks.Select(tl => tl.Ticket.TicketNumber).ToList(),
                TicketStatus = b.TicketLinks.FirstOrDefault()?.Ticket.Status?.Name,
                TestCaseNumber = b.TestCaseNumber, // Assuming added to BacklogItem
                IsCoverageGap = !b.TicketLinks.Any()
            });
        }

        public async Task<CostingReportDto> GetCostingReportAsync(int projectId, int? subProjectId = null, int? productId = null)
        {
            var budget = await _budgetRepository.Query().FirstOrDefaultAsync(b => b.ProjectId == projectId);
            var contractValue = budget?.ContractValue ?? 0;
            var budgetAmount = budget?.BudgetAmount ?? 0;

            // Total Cost per ticket = time logged × assignee hourly rate
            var timeLogs = await _timeLogRepository.Query()
                .Include(tl => tl.Ticket)
                .Include(tl => tl.User)
                .Where(tl => tl.Ticket.ProjectId == projectId)
                .ToListAsync();

            decimal totalCost = 0;
            foreach (var log in timeLogs)
            {
                var rate = await _userRateRepository.Query()
                    .Where(r => r.UserId == log.UserId && r.EffectiveFrom <= log.LoggedAt && (r.EffectiveTo == null || r.EffectiveTo >= log.LoggedAt))
                    .FirstOrDefaultAsync();

                totalCost += log.HoursLogged * (rate?.HourlyRate ?? 0);
            }

            var profitLoss = contractValue - totalCost;
            var margin = contractValue > 0 ? (profitLoss / contractValue) * 100 : 0;

            // Burn rate = totalCost / days since project start
            var project = await _ticketRepository.Query().Where(t => t.ProjectId == projectId).Select(t => t.Project).FirstOrDefaultAsync();
            var days = (DateTime.UtcNow - (project?.StartDate ?? DateTime.UtcNow)).Days;
            var burnRate = days > 0 ? totalCost / days : 0;

            return new CostingReportDto
            {
                TotalCost = totalCost,
                BudgetAmount = budgetAmount,
                ContractValue = contractValue,
                ProfitLoss = profitLoss,
                StatusColor = profitLoss >= 0 ? "green" : "red",
                MarginPercentage = margin,
                BurnRate = burnRate,
                CostForecast = burnRate * (days + 30) // Simple 30-day forecast
            };
        }

        public async Task<IEnumerable<DelayReportItemDto>> GetDelayReportAsync(int projectId)
        {
            var delays = await _delayRepository.Query()
                .Include(d => d.Ticket).ThenInclude(t => t.SubProject)
                .Include(d => d.Ticket).ThenInclude(t => t.Assignees).ThenInclude(a => a.User)
                .Where(d => d.Ticket.ProjectId == projectId)
                .ToListAsync();

            return delays.Select(d => new DelayReportItemDto
            {
                TicketNumber = d.Ticket.TicketNumber,
                Title = d.Ticket.Title,
                SubProjectName = d.Ticket.SubProject?.Name ?? "Unknown",
                AssigneeName = d.Ticket.Assignees.FirstOrDefault()?.User.DisplayName ?? "Unassigned",
                OriginalDueDate = d.Ticket.ExpectedDueDate,
                RevisedDueDate = d.RevisedDueDate,
                DaysDelayed = d.DaysDelayed,
                DelayType = d.DelayType.ToString(),
                Reason = d.Reason
            });
        }

        public async Task<DependencyMatrixDto> GetDependencyMatrixAsync(int projectId, int? subProjectId = null, int? sprintId = null)
        {
            var links = await _ticketLinkRepository.Query()
                .Include(l => l.SourceTicket)
                .Include(l => l.TargetTicket)
                .Where(l => l.SourceTicket.ProjectId == projectId)
                .ToListAsync();

            return new DependencyMatrixDto
            {
                Dependencies = links.Select(l => new DependencyItemDto
                {
                    FromId = l.SourceTicketId,
                    FromLabel = l.SourceTicket.TicketNumber,
                    ToId = l.TargetTicketId,
                    ToLabel = l.TargetTicket.TicketNumber,
                    IsBlocked = l.LinkType == TicketLinkType.Blocks // Simplified enum access
                }).ToList()
            };
        }

        public async Task<byte[]> ExportRtmAsync(int projectId, string format) => await Task.FromResult(new byte[0]);
        public async Task<byte[]> ExportDependencyMatrixAsync(int projectId, string format) => await Task.FromResult(new byte[0]);
        public async Task<byte[]> ExportCostingReportAsync(int projectId, string format) => await Task.FromResult(new byte[0]);
        public async Task<byte[]> ExportDelayReportAsync(int projectId, string format) => await Task.FromResult(new byte[0]);

        public async Task<object> GetSprintReportAsync(int sprintId) => await Task.FromResult(new object());
        public async Task<object> GetBugReportAsync(int projectId) => await Task.FromResult(new object());
        public async Task<object> GetWorkloadReportAsync(int projectId, DateTime from, DateTime to) => await Task.FromResult(new object());
        public async Task<object> GetTicketAgeReportAsync(int projectId) => await Task.FromResult(new object());
        public async Task<object> GetChangeRequestLogAsync(int projectId) => await Task.FromResult(new object());

        public async Task<TimeLogDto> LogTimeAsync(int ticketId, CreateTimeLogRequest request, int userId)
        {
            var log = new TimeLog
            {
                TicketId = ticketId,
                UserId = userId,
                HoursLogged = request.HoursLogged,
                Description = request.Description,
                LoggedAt = DateTime.UtcNow
            };
            await _timeLogRepository.AddAsync(log);
            return new TimeLogDto
            {
                Id = log.Id,
                UserId = userId,
                HoursLogged = log.HoursLogged,
                LoggedAt = log.LoggedAt,
                Description = log.Description
            };
        }

        public async Task<IEnumerable<TimeLogDto>> GetTicketTimeLogsAsync(int ticketId)
        {
            var logs = await _timeLogRepository.Query().Include(tl => tl.User).Where(tl => tl.TicketId == ticketId).ToListAsync();
            return logs.Select(l => new TimeLogDto
            {
                Id = l.Id,
                UserId = l.UserId,
                UserDisplayName = l.User.DisplayName,
                HoursLogged = l.HoursLogged,
                LoggedAt = l.LoggedAt,
                Description = l.Description
            });
        }

        public async Task DeleteTimeLogAsync(int logId, int userId)
        {
            var log = await _timeLogRepository.GetByIdAsync(logId);
            if (log != null && log.UserId == userId) await _timeLogRepository.DeleteAsync(logId);
        }

        public async Task SetBudgetAsync(int projectId, SetProjectBudgetRequest request, int userId)
        {
            var existing = await _budgetRepository.Query().FirstOrDefaultAsync(b => b.ProjectId == projectId);
            if (existing == null)
            {
                await _budgetRepository.AddAsync(new ProjectBudget
                {
                    ProjectId = projectId,
                    BudgetAmount = request.BudgetAmount,
                    ContractValue = request.ContractValue,
                    SetByUserId = userId,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existing.BudgetAmount = request.BudgetAmount;
                existing.ContractValue = request.ContractValue;
                await _budgetRepository.UpdateAsync(existing);
            }
        }

        public async Task<ProjectBudgetDto?> GetBudgetAsync(int projectId)
        {
            var budget = await _budgetRepository.Query().FirstOrDefaultAsync(b => b.ProjectId == projectId);
            return budget != null ? new ProjectBudgetDto { BudgetAmount = budget.BudgetAmount, ContractValue = budget.ContractValue } : null;
        }
    }
}
