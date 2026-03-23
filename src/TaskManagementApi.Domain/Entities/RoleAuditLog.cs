namespace TaskManagementApi.Domain.Entities
{
    public class RoleAuditLog : BaseEntity
    {
        public int RoleId { get; set; }
        public string Action { get; set; } = null!;       // e.g. "PERMISSION_ADDED", "ROLE_CREATED"
        public string ChangedByUserId { get; set; } = null!;
        public string Details { get; set; } = null!;      // JSON string describing what changed
        public DateTime ChangedAt { get; set; }
    }
}
