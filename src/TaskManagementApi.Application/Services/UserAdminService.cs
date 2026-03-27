using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Application.DTOs.UserAdmin;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Application.Services
{
    public class UserAdminService : IUserAdminService
    {
        private readonly IRepository<UserInvitation> _invitationRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Team> _teamRepository;
        private readonly IRepository<TeamMember> _memberRepository;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;

        public UserAdminService(
            IRepository<UserInvitation> invitationRepository,
            IRepository<User> userRepository,
            IRepository<Team> teamRepository,
            IRepository<TeamMember> memberRepository,
            UserManager<User> userManager,
            IEmailService emailService,
            ISmsService smsService)
        {
            _invitationRepository = invitationRepository;
            _userRepository = userRepository;
            _teamRepository = teamRepository;
            _memberRepository = memberRepository;
            _userManager = userManager;
            _emailService = emailService;
            _smsService = smsService;
        }

        public async Task<InvitationDto> InviteUserAsync(InviteUserRequest request, int userId)
        {
            var invitation = new UserInvitation
            {
                Email = request.Email,
                InvitedByUserId = userId,
                ProjectId = request.ProjectId,
                RoleId = request.RoleId,
                Token = Guid.NewGuid(),
                ExpiresAt = DateTime.UtcNow.AddHours(48),
                Status = InvitationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _invitationRepository.AddAsync(invitation);

            var link = $"https://app.taskapi.com/setup-account?token={invitation.Token}";
            await _emailService.SendEmailAsync(invitation.Email, "Account Setup", $"Welcome! Set up your account here: {link}");

            return MapToDto(invitation);
        }

        public async Task AcceptInvitationAsync(AcceptInvitationRequest request)
        {
            var invitation = await _invitationRepository.Query().FirstOrDefaultAsync(i => i.Token == request.Token && i.Status == InvitationStatus.Pending);
            if (invitation == null || invitation.ExpiresAt < DateTime.UtcNow)
                throw new InvalidOperationException("Invalid or expired invitation token");

            var user = new User
            {
                UserName = invitation.Email,
                Email = invitation.Email,
                DisplayName = request.DisplayName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Provider = "Local",
                ProviderId = Guid.NewGuid().ToString() // or use Token
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded) throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            invitation.Status = InvitationStatus.Accepted;
            invitation.AcceptedAt = DateTime.UtcNow;
            await _invitationRepository.UpdateAsync(invitation);
        }

        public async Task DeactivateUserAsync(int id, int userId)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return;

            user.IsActive = false;
            user.DeactivatedAt = DateTime.UtcNow;
            user.DeactivatedByUserId = userId;
            await _userRepository.UpdateAsync(user);
        }

        public async Task<TeamDto> CreateTeamAsync(int projectId, CreateTeamRequest request)
        {
            var team = new Team
            {
                Name = request.Name,
                Description = request.Description,
                ProjectId = projectId,
                CreatedAt = DateTime.UtcNow
            };
            await _teamRepository.AddAsync(team);
            return new TeamDto { Id = team.Id, Name = team.Name, Description = team.Description, ProjectId = projectId };
        }

        public async Task SeedDefaultTeamsAsync(int projectId)
        {
            var defaults = new[] { "Development", "QA", "BA", "DevOps", "Design" };
            foreach (var name in defaults)
            {
                await CreateTeamAsync(projectId, new CreateTeamRequest { Name = name });
            }
        }

        public async Task SubmitMobileAsync(int userId, string mobileNumber)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;

            user.MobileNumber = mobileNumber;
            user.MobileVerified = false;
            user.MobileVerificationCode = new Random().Next(100000, 999999).ToString();
            await _userRepository.UpdateAsync(user);

            await _smsService.SendSmsAsync(mobileNumber, $"Your OTP code is {user.MobileVerificationCode}");
        }

        public async Task<bool> VerifyMobileAsync(int userId, string code)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null && user.MobileVerificationCode == code)
            {
                user.MobileVerified = true;
                user.MobileVerificationCode = null;
                await _userRepository.UpdateAsync(user);
                return true;
            }
            return false;
        }

        // Other methods implementation...
        public async Task<IEnumerable<InvitationDto>> GetPendingInvitationsAsync() => (await _invitationRepository.Query().Where(i => i.Status == InvitationStatus.Pending).ToListAsync()).Select(MapToDto);
        public async Task RevokeInvitationAsync(int id) { var i = await _invitationRepository.GetByIdAsync(id); if (i != null) { i.Status = InvitationStatus.Revoked; await _invitationRepository.UpdateAsync(i); } }
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync() => (await _userRepository.GetAllAsync()).Select(MapToUserDto);
        public async Task<UserDto?> GetUserByIdAsync(int id) { var u = await _userRepository.GetByIdAsync(id); return u != null ? MapToUserDto(u) : null; }
        public async Task ReactivateUserAsync(int id) { var u = await _userRepository.GetByIdAsync(id); if (u != null) { u.IsActive = true; u.DeactivatedAt = null; await _userRepository.UpdateAsync(u); } }
        public async Task UpdateProfileAsync(int userId, UpdateProfileRequest request) { var u = await _userRepository.GetByIdAsync(userId); if (u != null) { u.DisplayName = request.DisplayName; u.AvatarUrl = request.AvatarUrl; await _userRepository.UpdateAsync(u); } }
        public async Task<IEnumerable<TeamDto>> GetProjectTeamsAsync(int projectId) => (await _teamRepository.Query().Where(t => t.ProjectId == projectId).ToListAsync()).Select(t => new TeamDto { Id = t.Id, Name = t.Name, Description = t.Description, ProjectId = projectId });
        public async Task UpdateTeamAsync(int id, CreateTeamRequest request) { var t = await _teamRepository.GetByIdAsync(id); if (t != null) { t.Name = request.Name; t.Description = request.Description; await _teamRepository.UpdateAsync(t); } }
        public async Task DeleteTeamAsync(int id) => await _teamRepository.DeleteAsync(id);
        public async Task AddMemberToTeamAsync(int teamId, int userId) => await _memberRepository.AddAsync(new TeamMember { TeamId = teamId, UserId = userId, JoinedAt = DateTime.UtcNow });
        public async Task RemoveMemberFromTeamAsync(int teamId, int userId) { /* Logic to remove */ }
        public async Task<IEnumerable<UserDto>> GetTeamMembersAsync(int teamId) => (await _memberRepository.Query().Include(m => m.User).Where(m => m.TeamId == teamId).Select(m => m.User).ToListAsync()).Select(MapToUserDto);

        private InvitationDto MapToDto(UserInvitation i) => new InvitationDto { Id = i.Id, Email = i.Email, Token = i.Token, ExpiresAt = i.ExpiresAt, Status = i.Status };
        private UserDto MapToUserDto(User u) => new UserDto { Id = u.Id, Email = u.Email!, DisplayName = u.DisplayName, IsActive = u.IsActive, MobileNumber = u.MobileNumber, MobileVerified = u.MobileVerified, HourlyRate = u.HourlyRate };
    }
}
