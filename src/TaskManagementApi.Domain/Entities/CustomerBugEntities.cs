using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Domain.Entities
{
    public enum BugParseStatus
    {
        Parsed,
        InvalidFormat,
        Duplicate
    }

    public class CustomerBugSubmission : BaseEntity
    {
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public string SenderEmail { get; set; } = null!;
        public string SenderName { get; set; } = null!;
        public string RawEmailBody { get; set; } = null!;

        public string? ParsedTitle { get; set; }
        public string? ParsedDescription { get; set; }
        public string? ParsedStepsToReproduce { get; set; }
        public string? ParsedExpectedBehaviour { get; set; }
        public string? ParsedActualBehaviour { get; set; }
        public string? ParsedEnvironment { get; set; }
        public string? ParsedSeverity { get; set; }

        public DateTime ReceivedAt { get; set; }
        public BugParseStatus ParseStatus { get; set; }

        public int? CreatedTicketId { get; set; }
        public Ticket? CreatedTicket { get; set; }
    }

    public class BugApprovalSla : BaseEntity
    {
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public int SlaBusinessDays { get; set; } = 2;
        public int EscalateAfterDays { get; set; }
    }
}
