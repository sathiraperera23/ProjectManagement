namespace TaskManagementApi.Application.DTOs.Tickets
{
    public class CommentDto
    {
        public int Id { get; set; }
        public int? ParentCommentId { get; set; }
        public string Body { get; set; } = null!;
        public int AuthorId { get; set; }
        public string AuthorDisplayName { get; set; } = null!;
        public bool IsInternalNote { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<CommentReactionDto> Reactions { get; set; } = new();
        public List<CommentDto> Replies { get; set; } = new();
    }

    public class CreateCommentRequest
    {
        public int? ParentCommentId { get; set; }
        public string Body { get; set; } = null!;
        public bool IsInternalNote { get; set; }
    }

    public class UpdateCommentRequest
    {
        public string Body { get; set; } = null!;
    }

    public class CommentReactionDto
    {
        public string Emoji { get; set; } = null!;
        public int UserId { get; set; }
    }

    public class TicketAttachmentDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = null!;
        public long FileSizeBytes { get; set; }
        public string? ContentType { get; set; }
        public string? DownloadUrl { get; set; }
        public string? ExternalUrl { get; set; }
        public string? ExternalLabel { get; set; }
        public int UploadedByUserId { get; set; }
        public string UploadedByUserName { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
        public int Version { get; set; }
    }

    public class LinkExternalAttachmentRequest
    {
        public string ExternalUrl { get; set; } = null!;
        public string ExternalLabel { get; set; } = null!;
    }

    public class DailyUpdateDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserDisplayName { get; set; } = null!;
        public string WorkedOn { get; set; } = null!;
        public string PlannedNext { get; set; } = null!;
        public string? Blockers { get; set; }
        public DateTime SubmittedAt { get; set; }
        public List<int> LinkedTicketIds { get; set; } = new();
    }

    public class CreateDailyUpdateRequest
    {
        public int ProjectId { get; set; }
        public string WorkedOn { get; set; } = null!;
        public string PlannedNext { get; set; } = null!;
        public string? Blockers { get; set; }
    }
}
