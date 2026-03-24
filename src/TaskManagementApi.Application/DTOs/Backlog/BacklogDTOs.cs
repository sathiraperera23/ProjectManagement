using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Application.DTOs.Backlog
{
    public class UserSummaryDto
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? AvatarUrl { get; set; }
    }

    public class BacklogItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string Priority { get; set; } = null!;
        public int? ProjectId { get; set; }
        public int? ProductId { get; set; }
        public int Order { get; set; }
        public string? AcceptanceCriteria { get; set; }
        public UserSummaryDto Owner { get; set; } = null!;
        public UserSummaryDto? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int VersionCount { get; set; }
        public int AttachmentCount { get; set; }
        public int LinkedTicketCount { get; set; }
        public List<BacklogAttachmentDto> Attachments { get; set; } = new();
        public List<BacklogItemTicketLinkDto> TicketLinks { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class BacklogItemVersionDto
    {
        public int Id { get; set; }
        public int VersionNumber { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? AcceptanceCriteria { get; set; }
        public string ChangeNote { get; set; } = null!;
        public UserSummaryDto CreatedBy { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class BacklogAttachmentDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long FileSizeBytes { get; set; }
        public string DownloadUrl { get; set; } = null!;
        public UserSummaryDto UploadedBy { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
    }

    public class BacklogItemTicketLinkDto
    {
        public int TicketId { get; set; }
        public string TicketNumber { get; set; } = null!;
        public string TicketTitle { get; set; } = null!;
        public string TicketStatus { get; set; } = null!;
    }

    public class BacklogApprovalRequestDto
    {
        public int Id { get; set; }
        public int BacklogItemId { get; set; }
        public string Status { get; set; } = null!;
        public string? ReviewNote { get; set; }
        public UserSummaryDto RequestedBy { get; set; } = null!;
        public UserSummaryDto? ReviewedBy { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }

    public class CreateBacklogItemRequest
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public BacklogItemType Type { get; set; }
        public BacklogItemPriority Priority { get; set; }
        public int? ProjectId { get; set; }
        public int? ProductId { get; set; }
        public string? AcceptanceCriteria { get; set; }
    }

    public class UpdateBacklogItemRequest
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public BacklogItemPriority Priority { get; set; }
        public string? AcceptanceCriteria { get; set; }
        public string ChangeNote { get; set; } = null!;
    }

    public class BacklogFilterRequest
    {
        public BacklogItemType? Type { get; set; }
        public BacklogItemStatus? Status { get; set; }
        public BacklogItemPriority? Priority { get; set; }
        public int? OwnerId { get; set; }
        public string? Search { get; set; }
    }

    public class ReorderBacklogRequest
    {
        public List<BacklogItemOrderDto> Items { get; set; } = new();
    }

    public class BacklogItemOrderDto
    {
        public int Id { get; set; }
        public int Order { get; set; }
    }
}
