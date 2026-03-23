using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Application.DTOs.Tickets
{
    public class CreateTicketRequest
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public TicketCategory Category { get; set; }
        public TicketPriority Priority { get; set; }
        public int ProjectId { get; set; }
        public int ProductId { get; set; }
        public int SubProjectId { get; set; }
        public int? TeamId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpectedDueDate { get; set; }
        public int? StoryPoints { get; set; }
        public int? SprintId { get; set; }
        public int? MilestoneId { get; set; }
        public string? BrdNumber { get; set; }
        public string? UseCaseNumber { get; set; }
        public string? TestCaseNumber { get; set; }
        public TicketSeverity? Severity { get; set; }
        public string? StepsToReproduce { get; set; }
        public string? ExpectedBehaviour { get; set; }
        public string? ActualBehaviour { get; set; }
        public TicketEnvironment? Environment { get; set; }
        public List<int> AssigneeIds { get; set; } = new();
        public List<string> Labels { get; set; } = new();
    }

    public class UpdateTicketRequest
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public TicketCategory Category { get; set; }
        public TicketPriority Priority { get; set; }
        public int? TeamId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpectedDueDate { get; set; }
        public int? StoryPoints { get; set; }
        public int? SprintId { get; set; }
        public int? MilestoneId { get; set; }
        public string? BrdNumber { get; set; }
        public string? UseCaseNumber { get; set; }
        public string? TestCaseNumber { get; set; }
        public TicketSeverity? Severity { get; set; }
        public string? StepsToReproduce { get; set; }
        public string? ExpectedBehaviour { get; set; }
        public string? ActualBehaviour { get; set; }
        public TicketEnvironment? Environment { get; set; }
        public List<int> AssigneeIds { get; set; } = new();
        public List<string> Labels { get; set; } = new();
    }

    public class TicketResponse
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public TicketCategory Category { get; set; }
        public TicketPriority Priority { get; set; }
        public int ProjectId { get; set; }
        public int ProductId { get; set; }
        public int SubProjectId { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; } = null!;
        public int? TeamId { get; set; }
        public int ReporterId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpectedDueDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public int? StoryPoints { get; set; }
        public List<int> AssigneeIds { get; set; } = new();
        public List<string> Labels { get; set; } = new();
    }

    public class UpdateTicketStatusRequest
    {
        public int StatusId { get; set; }
        public string? PauseReason { get; set; }
        public string? CancelReason { get; set; }
    }

    public class BulkUpdateStatusRequest
    {
        public List<int> TicketIds { get; set; } = new();
        public int StatusId { get; set; }
    }

    public class BulkAssignRequest
    {
        public List<int> TicketIds { get; set; } = new();
        public List<int> AssigneeIds { get; set; } = new();
    }

    public class BulkUpdatePriorityRequest
    {
        public List<int> TicketIds { get; set; } = new();
        public TicketPriority Priority { get; set; }
    }

    public class LinkTicketRequest
    {
        public int TargetTicketId { get; set; }
        public TicketLinkType LinkType { get; set; }
    }

    public class TicketStatusHistoryResponse
    {
        public int FromStatusId { get; set; }
        public string FromStatusName { get; set; } = null!;
        public int ToStatusId { get; set; }
        public string ToStatusName { get; set; } = null!;
        public int ChangedByUserId { get; set; }
        public string ChangedByUserName { get; set; } = null!;
        public DateTime ChangedAt { get; set; }
    }

    public class TicketStatusResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Colour { get; set; }
        public int Order { get; set; }
        public bool IsDefault { get; set; }
        public bool IsTerminal { get; set; }
    }

    public class CreateTicketStatusRequest
    {
        public string Name { get; set; } = null!;
        public string? Colour { get; set; }
        public int Order { get; set; }
        public bool IsDefault { get; set; }
        public bool IsTerminal { get; set; }
    }
}
