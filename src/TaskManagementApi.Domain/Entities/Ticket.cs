using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Domain.Entities
{
    public class Ticket : BaseEntity
    {
        public string TicketNumber { get; set; } = null!; // Auto-generated: ProjectCode + sequence
        public string Title { get; set; } = null!;
        public string? Description { get; set; } // Rich text HTML string

        public TicketCategory Category { get; set; }
        public TicketPriority Priority { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int SubProjectId { get; set; }
        public SubProject SubProject { get; set; } = null!;

        public int StatusId { get; set; }
        public TicketStatus Status { get; set; } = null!;

        public int? TeamId { get; set; }
        public Team? Team { get; set; }
        public int ReporterId { get; set; }
        public User Reporter { get; set; } = null!;

        public DateTime? StartDate { get; set; }
        public DateTime? ExpectedDueDate { get; set; }
        public DateTime? ActualEndDate { get; set; } // Auto-set when terminal status

        public int? StoryPoints { get; set; }
        public int? SprintId { get; set; }
        public int? MilestoneId { get; set; }

        public string? BrdNumber { get; set; }
        public string? UseCaseNumber { get; set; }
        public string? TestCaseNumber { get; set; }

        // Bug-specific fields
        public TicketSeverity? Severity { get; set; }
        public string? StepsToReproduce { get; set; }
        public string? ExpectedBehaviour { get; set; }
        public string? ActualBehaviour { get; set; }
        public TicketEnvironment? Environment { get; set; }

        public ApprovalStatus? ApprovalStatus { get; set; } // For client-reported bugs

        public string? PauseReason { get; set; } // Required when Paused
        public string? CancelReason { get; set; } // Required when Cancelled

        // Navigation properties
        public ICollection<TicketAssignee> Assignees { get; set; } = new List<TicketAssignee>();
        public ICollection<TicketLabel> Labels { get; set; } = new List<TicketLabel>();
        public ICollection<TicketLink> OutgoingLinks { get; set; } = new List<TicketLink>();
        public ICollection<TicketLink> IncomingLinks { get; set; } = new List<TicketLink>();
        public ICollection<TicketStatusHistory> StatusHistory { get; set; } = new List<TicketStatusHistory>();
    }
}
