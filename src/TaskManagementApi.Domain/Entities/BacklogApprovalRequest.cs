using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Domain.Entities
{
    public class BacklogApprovalRequest
    {
        public int Id { get; set; }
        public int BacklogItemId { get; set; }
        public BacklogItem BacklogItem { get; set; } = null!;
        public int RequestedByUserId { get; set; }
        public User RequestedBy { get; set; } = null!;
        public int? ReviewedByUserId { get; set; }
        public User? ReviewedBy { get; set; }
        public ApprovalRequestStatus Status { get; set; }
        public string? ReviewNote { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
