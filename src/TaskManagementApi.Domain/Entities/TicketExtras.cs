using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementApi.Domain.Entities
{
    public class TicketComment : BaseEntity
    {
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        public int? ParentCommentId { get; set; }
        public TicketComment? ParentComment { get; set; }

        public string Body { get; set; } = null!; // HTML rich text
        public int AuthorId { get; set; }
        public User Author { get; set; } = null!;

        public bool IsInternalNote { get; set; } // visible to team only, not guest viewers

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<TicketComment> Replies { get; set; } = new List<TicketComment>();
        public ICollection<CommentMention> Mentions { get; set; } = new List<CommentMention>();
        public ICollection<CommentReaction> Reactions { get; set; } = new List<CommentReaction>();
    }

    public class CommentMention
    {
        public int CommentId { get; set; }
        public TicketComment Comment { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }

    public class CommentReaction : BaseEntity
    {
        public int CommentId { get; set; }
        public TicketComment Comment { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string Emoji { get; set; } = null!;
    }

    public class TicketWatcher
    {
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }

    public class TicketAttachment : BaseEntity
    {
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        public string FileName { get; set; } = null!;
        public string? FilePath { get; set; }
        public string? ContentType { get; set; }
        public long FileSizeBytes { get; set; }

        public string? ExternalUrl { get; set; }
        public string? ExternalLabel { get; set; }

        public int UploadedByUserId { get; set; }
        public User UploadedByUser { get; set; } = null!;

        public DateTime UploadedAt { get; set; }
        public int Version { get; set; } // increments on re-upload of same filename
    }

    public class DailyUpdate : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public string WorkedOn { get; set; } = null!;
        public string PlannedNext { get; set; } = null!;
        public string? Blockers { get; set; }

        public DateTime SubmittedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<DailyUpdateTicketLink> TicketLinks { get; set; } = new List<DailyUpdateTicketLink>();
    }

    public class DailyUpdateTicketLink
    {
        public int DailyUpdateId { get; set; }
        public DailyUpdate DailyUpdate { get; set; } = null!;

        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;
    }
}
