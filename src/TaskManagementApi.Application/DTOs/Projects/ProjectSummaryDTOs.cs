using TaskManagementApi.Domain.Entities;

namespace TaskManagementApi.Application.DTOs.Projects
{
    public class ProjectSummaryDto
    {
        public ProjectHeaderDto Header { get; set; } = null!;
        public WipStatusDto WipStatus { get; set; } = null!;
        public List<StatusGroupDto> ProgressOverview { get; set; } = new();
        public List<MilestoneSummaryDto> Milestones { get; set; } = new();
        public SprintSummaryDto? ActiveSprint { get; set; }
        public List<TeamActivityDto> TeamActivity { get; set; } = new();
        public List<RecentTicketDto> RecentTickets { get; set; } = new();
        public DelaySummaryDto DelaySummary { get; set; } = null!;
        public decimal BudgetConsumedPercentage { get; set; }
    }

    public class ProjectHeaderDto
    {
        public string ProjectName { get; set; } = null!;
        public string? Client { get; set; }
        public string? ProjectManager { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public string Status { get; set; } = null!;
        public int DaysRemaining { get; set; }
    }

    public class WipStatusDto
    {
        public int Count { get; set; }
        public string IndicatorLevel { get; set; } = "Green"; // Green, Amber, Red
    }

    public class StatusGroupDto
    {
        public string StatusName { get; set; } = null!;
        public int Count { get; set; }
    }

    public class MilestoneSummaryDto
    {
        public string Name { get; set; } = null!;
        public DateTime TargetDate { get; set; }
        public double CompletionPercentage { get; set; }
        public string Status { get; set; } = "OnTrack"; // OnTrack, AtRisk, Overdue
    }

    public class SprintSummaryDto
    {
        public string Name { get; set; } = null!;
        public string? Goal { get; set; }
        public int DaysRemaining { get; set; }
        public double CompletionPercentage { get; set; }
    }

    public class TeamActivityDto
    {
        public string TeamName { get; set; } = null!;
        public int UpdateCount { get; set; }
    }

    public class RecentTicketDto
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime UpdatedAt { get; set; }
    }

    public class DelaySummaryDto
    {
        public int OverdueCount { get; set; }
        public int BlockedCount { get; set; }
    }

    public class SetWipLimitRequest
    {
        public int? SubProjectId { get; set; }
        public int MaxWip { get; set; }
    }

    public class AccessRuleDto
    {
        public int Id { get; set; }
        public AccessComponentType ComponentType { get; set; }
        public int ComponentId { get; set; }
        public AccessConditionType ConditionType { get; set; }
        public string ConditionValue { get; set; } = null!;
        public AccessLevel AccessLevel { get; set; }
        public bool IsOverride { get; set; }
    }

    public class CreateAccessRuleRequest
    {
        public AccessComponentType ComponentType { get; set; }
        public int ComponentId { get; set; }
        public AccessConditionType ConditionType { get; set; }
        public string ConditionValue { get; set; } = null!;
        public AccessLevel AccessLevel { get; set; }
        public bool IsOverride { get; set; }
    }

    public class AccessRequestDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserDisplayName { get; set; } = null!;
        public AccessComponentType ComponentType { get; set; }
        public int ComponentId { get; set; }
        public AccessLevel RequestedAccessLevel { get; set; }
        public string? RequestNote { get; set; }
        public DateTime RequestedAt { get; set; }
    }
}
