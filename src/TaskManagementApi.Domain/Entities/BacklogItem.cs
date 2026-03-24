using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Domain.Entities
{
    public class BacklogItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public BacklogItemType Type { get; set; }
        public BacklogItemStatus Status { get; set; }
        public BacklogItemPriority Priority { get; set; }
        public int? ProjectId { get; set; }
        public Project? Project { get; set; }
        public int? ProductId { get; set; }
        public Product? Product { get; set; }
        public int OwnerId { get; set; }
        public User Owner { get; set; } = null!;
        public int Order { get; set; }
        public string? AcceptanceCriteria { get; set; }
        public int? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ICollection<BacklogItemVersion> Versions { get; set; } = new List<BacklogItemVersion>();
        public ICollection<BacklogAttachment> Attachments { get; set; } = new List<BacklogAttachment>();
        public ICollection<BacklogItemTicketLink> TicketLinks { get; set; } = new List<BacklogItemTicketLink>();
        public ICollection<BacklogApprovalRequest> ApprovalRequests { get; set; } = new List<BacklogApprovalRequest>();
    }
}
