using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Application.DTOs.Roles;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Constants;
using TaskManagementApi.Web.Authorization;
using TaskManagementApi.Application.Validators;
using System.Security.Claims;

namespace TaskManagementApi.Web.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/user-roles")]
    [Authorize]
    public class UserRolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public UserRolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        private string GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
        {
            var validator = new AssignRoleRequestValidator();
            var result = await validator.ValidateAsync(request);
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(e => e.ErrorMessage));

            await _roleService.AssignRoleToUserAsync(request, GetCurrentUserId());
            return NoContent();
        }

        [HttpDelete("{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveRole(int projectId, int userId)
        {
            await _roleService.RemoveRoleFromUserAsync(userId, projectId, GetCurrentUserId());
            return NoContent();
        }

        [HttpGet("{userId}/permissions")]
        public async Task<IActionResult> GetUserPermissions(int projectId, int userId)
        {
            var permissions = await _roleService.GetEffectivePermissionsAsync(userId, projectId);
            return Ok(permissions);
        }
    }
}
