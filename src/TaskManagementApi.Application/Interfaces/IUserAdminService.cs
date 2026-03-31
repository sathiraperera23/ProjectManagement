using TaskManagementApi.Application.DTOs.UserAdmin;

namespace TaskManagementApi.Application.Interfaces
{
    public interface IUserAdminService
    {
        // Invitations
        Task<InvitationDto> InviteUserAsync(InviteUserRequest request, int userId);
        Task<IEnumerable<InvitationDto>> GetPendingInvitationsAsync();
        Task RevokeInvitationAsync(int id);
        Task AcceptInvitationAsync(AcceptInvitationRequest request);

        // User Management
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int id);
        Task DeactivateUserAsync(int id, int userId);
        Task ReactivateUserAsync(int id);
        Task UpdateProfileAsync(int userId, UpdateProfileRequest request);

        // Mobile Verification
        Task SubmitMobileAsync(int userId, string mobileNumber);
        Task<bool> VerifyMobileAsync(int userId, string code);

        // Team Management
        Task<TeamDto> CreateTeamAsync(int projectId, CreateTeamRequest request);
        Task<IEnumerable<TeamDto>> GetProjectTeamsAsync(int projectId);
        Task UpdateTeamAsync(int id, CreateTeamRequest request);
        Task DeleteTeamAsync(int id);
        Task AddMemberToTeamAsync(int teamId, int userId);
        Task RemoveMemberFromTeamAsync(int teamId, int userId);
        Task<IEnumerable<UserDto>> GetTeamMembersAsync(int teamId);

        Task SeedDefaultTeamsAsync(int projectId);
    }
}
