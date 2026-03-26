using TaskManagementApi.Domain.Entities;

namespace TaskManagementApi.Application.DTOs.Reports
{
    public class TimeLogDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserDisplayName { get; set; } = null!;
        public decimal HoursLogged { get; set; }
        public DateTime LoggedAt { get; set; }
        public string? Description { get; set; }
    }

    public class CreateTimeLogRequest
    {
        public decimal HoursLogged { get; set; }
        public string? Description { get; set; }
    }

    public class ProjectBudgetDto
    {
        public decimal BudgetAmount { get; set; }
        public decimal? ContractValue { get; set; }
    }

    public class SetProjectBudgetRequest
    {
        public decimal BudgetAmount { get; set; }
        public decimal? ContractValue { get; set; }
    }

    public class RtmReportItemDto
    {
        public string RequirementId { get; set; } = null!;
        public string RequirementDescription { get; set; } = null!;
        public string? UseCaseNumber { get; set; }
        public string? UserStory { get; set; }
        public List<string> LinkedTickets { get; set; } = new();
        public string? TicketStatus { get; set; }
        public string? TestCaseNumber { get; set; }
        public string? TestStatus { get; set; }
        public bool IsCoverageGap { get; set; }
    }

    public class DependencyMatrixDto
    {
        public List<DependencyItemDto> Dependencies { get; set; } = new();
    }

    public class DependencyItemDto
    {
        public int FromId { get; set; }
        public string FromLabel { get; set; } = null!;
        public int ToId { get; set; }
        public string ToLabel { get; set; } = null!;
        public bool IsBlocked { get; set; }
    }

    public class CostingReportDto
    {
        public decimal TotalCost { get; set; }
        public decimal BudgetAmount { get; set; }
        public decimal ContractValue { get; set; }
        public decimal ProfitLoss { get; set; }
        public string StatusColor { get; set; } = "green";
        public decimal MarginPercentage { get; set; }
        public decimal BurnRate { get; set; }
        public decimal CostForecast { get; set; }
    }

    public class DelayReportItemDto
    {
        public string TicketNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string SubProjectName { get; set; } = null!;
        public string AssigneeName { get; set; } = null!;
        public DateTime? OriginalDueDate { get; set; }
        public DateTime? RevisedDueDate { get; set; }
        public int DaysDelayed { get; set; }
        public string DelayType { get; set; } = null!;
        public string? Reason { get; set; }
    }
}
