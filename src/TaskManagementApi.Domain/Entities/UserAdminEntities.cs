using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Domain.Entities
{
    public enum InvitationStatus
    {
        Pending,
        Accepted,
        Expired,
        Revoked
    }

    public class UserInvitation : BaseEntity
    {
        public string Email { get; set; } = null!;
        public int InvitedByUserId { get; set; }
        public User InvitedByUser { get; set; } = null!;

        public int? ProjectId { get; set; }
        public Project? Project { get; set; }

        public int? RoleId { get; set; }
        public Role? Role { get; set; }

        public Guid Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public InvitationStatus Status { get; set; }
    }

    public class TeamMember
    {
        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime JoinedAt { get; set; }
    }
}
