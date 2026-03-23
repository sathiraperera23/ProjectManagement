namespace TaskManagementApi.Domain.Entities
{
    public class UserProjectRole : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
        public DateTime AssignedAt { get; set; }
        public int AssignedByUserId { get; set; }
    }
}
