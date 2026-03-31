using TaskManagementApi.Domain.Entities;

namespace TaskManagementApi.Application.DTOs.UserAdmin
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; }
        public string? MobileNumber { get; set; }
        public bool MobileVerified { get; set; }
        public decimal? HourlyRate { get; set; }
    }

    public class InviteUserRequest
    {
        public string Email { get; set; } = null!;
        public int? ProjectId { get; set; }
        public int? RoleId { get; set; }
    }

    public class InvitationDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public Guid Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public InvitationStatus Status { get; set; }
    }

    public class AcceptInvitationRequest
    {
        public Guid Token { get; set; }
        public string DisplayName { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class UpdateProfileRequest
    {
        public string DisplayName { get; set; } = null!;
        public string? AvatarUrl { get; set; }
    }

    public class TeamDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int ProjectId { get; set; }
    }

    public class CreateTeamRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
