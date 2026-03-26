using TaskManagementApi.Domain.Entities;

namespace TaskManagementApi.Application.DTOs.CustomerBugs
{
    public class BugSubmissionDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string SenderEmail { get; set; } = null!;
        public string SenderName { get; set; } = null!;
        public string? ParsedTitle { get; set; }
        public DateTime ReceivedAt { get; set; }
        public BugParseStatus ParseStatus { get; set; }
        public int? CreatedTicketId { get; set; }
    }

    public class BugApprovalSlaDto
    {
        public int ProjectId { get; set; }
        public int SlaBusinessDays { get; set; }
        public int EscalateAfterDays { get; set; }
    }

    public class UpdateBugSlaRequest
    {
        public int SlaBusinessDays { get; set; }
        public int EscalateAfterDays { get; set; }
    }

    public class BugApprovalRequest
    {
        public int? AssigneeId { get; set; }
    }

    public class BugRejectionRequest
    {
        public string Reason { get; set; } = null!;
    }

    public class BugInboundEmailRequest
    {
        public string To { get; set; } = null!;
        public string From { get; set; } = null!;
        public string FromName { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Html { get; set; } = null!;
    }
}
