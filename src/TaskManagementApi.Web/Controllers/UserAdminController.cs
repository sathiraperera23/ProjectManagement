using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Application.DTOs.UserAdmin;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Constants;
using TaskManagementApi.Web.Authorization;
using System.Security.Claims;

namespace TaskManagementApi.Web.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UserAdminController : ControllerBase
    {
        private readonly IUserAdminService _adminService;
        private readonly IUserManagerFacade _userManager;

        public UserAdminController(IUserAdminService adminService, IUserManagerFacade userManager)
        {
            _adminService = adminService;
            _userManager = userManager;
        }

        private async Task<int> GetCurrentUserId()
        {
            var providerId = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (providerId == null) return 0;
            var user = await _userManager.FindByProviderIdAsync(providerId);
            return user?.Id ?? 0;
        }

        [HttpPost("invite")]
        [RequirePermission(Permissions.ManageUsers)]
        public async Task<IActionResult> Invite([FromBody] InviteUserRequest request)
        {
            var invitation = await _adminService.InviteUserAsync(request, await GetCurrentUserId());
            return Ok(invitation);
        }

        [HttpGet("invitations")]
        [RequirePermission(Permissions.ManageUsers)]
        public async Task<IActionResult> GetPendingInvitations()
        {
            var items = await _adminService.GetPendingInvitationsAsync();
            return Ok(items);
        }

        [HttpDelete("invitations/{id}")]
        [RequirePermission(Permissions.ManageUsers)]
        public async Task<IActionResult> RevokeInvitation(int id)
        {
            await _adminService.RevokeInvitationAsync(id);
            return NoContent();
        }

        [HttpPost("accept-invitation")]
        [AllowAnonymous]
        public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationRequest request)
        {
            await _adminService.AcceptInvitationAsync(request);
            return Ok();
        }

        [HttpGet]
        [RequirePermission(Permissions.ManageUsers)]
        public async Task<IActionResult> GetAll()
        {
            var users = await _adminService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _adminService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPut("{id}/deactivate")]
        [RequirePermission(Permissions.ManageUsers)]
        public async Task<IActionResult> Deactivate(int id)
        {
            await _adminService.DeactivateUserAsync(id, await GetCurrentUserId());
            return NoContent();
        }

        [HttpPut("{id}/reactivate")]
        [RequirePermission(Permissions.ManageUsers)]
        public async Task<IActionResult> Reactivate(int id)
        {
            await _adminService.ReactivateUserAsync(id);
            return NoContent();
        }

        [HttpPut("me/profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            await _adminService.UpdateProfileAsync(await GetCurrentUserId(), request);
            return NoContent();
        }

        [HttpPut("me/mobile")]
        public async Task<IActionResult> SubmitMobile([FromQuery] string mobile)
        {
            await _adminService.SubmitMobileAsync(await GetCurrentUserId(), mobile);
            return NoContent();
        }

        [HttpPost("me/mobile/verify")]
        public async Task<IActionResult> VerifyMobile([FromQuery] string code)
        {
            var success = await _adminService.VerifyMobileAsync(await GetCurrentUserId(), code);
            return success ? Ok() : BadRequest("Invalid code");
        }
    }

    [ApiController]
    [Route("api/projects/{projectId}/teams")]
    [Authorize]
    public class TeamController : ControllerBase
    {
        private readonly IUserAdminService _adminService;

        public TeamController(IUserAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpPost]
        [RequirePermission(Permissions.ManageUsers)]
        public async Task<IActionResult> CreateTeam(int projectId, [FromBody] CreateTeamRequest request)
        {
            var team = await _adminService.CreateTeamAsync(projectId, request);
            return Ok(team);
        }

        [HttpGet]
        public async Task<IActionResult> GetTeams(int projectId)
        {
            var teams = await _adminService.GetProjectTeamsAsync(projectId);
            return Ok(teams);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTeam(int id, [FromBody] CreateTeamRequest request)
        {
            await _adminService.UpdateTeamAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeam(int id)
        {
            await _adminService.DeleteTeamAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/members")]
        public async Task<IActionResult> AddMember(int id, [FromQuery] int userId)
        {
            await _adminService.AddMemberToTeamAsync(id, userId);
            return NoContent();
        }

        [HttpDelete("{id}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(int id, int userId)
        {
            await _adminService.RemoveMemberFromTeamAsync(id, userId);
            return NoContent();
        }

        [HttpGet("{id}/members")]
        public async Task<IActionResult> GetMembers(int id)
        {
            var members = await _adminService.GetTeamMembersAsync(id);
            return Ok(members);
        }
    }
}
