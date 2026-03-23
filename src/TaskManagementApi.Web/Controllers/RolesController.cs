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
    [Route("api/roles")]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        private string GetCurrentUserId() => User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";

        [HttpGet]
        [RequirePermission(Permissions.ManageRoles)]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        [RequirePermission(Permissions.ManageRoles)]
        public async Task<IActionResult> GetById(int id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null) return NotFound();
            return Ok(role);
        }

        [HttpPost]
        [RequirePermission(Permissions.ManageRoles)]
        public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
        {
            var validator = new CreateRoleRequestValidator();
            var result = await validator.ValidateAsync(request);
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(e => e.ErrorMessage));

            var role = await _roleService.CreateRoleAsync(request, GetCurrentUserId());
            return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
        }

        [HttpPut("{id}")]
        [RequirePermission(Permissions.ManageRoles)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRoleRequest request)
        {
            var role = await _roleService.UpdateRoleAsync(id, request, GetCurrentUserId());
            return Ok(role);
        }

        [HttpDelete("{id}")]
        [RequirePermission(Permissions.ManageRoles)]
        public async Task<IActionResult> Delete(int id)
        {
            await _roleService.DeleteRoleAsync(id, GetCurrentUserId());
            return NoContent();
        }

        [HttpPut("{id}/permissions")]
        [RequirePermission(Permissions.ManageRoles)]
        public async Task<IActionResult> UpdatePermissions(int id, [FromBody] UpdateRolePermissionsRequest request)
        {
            await _roleService.UpdateRolePermissionsAsync(id, request, GetCurrentUserId());
            return NoContent();
        }

        [HttpGet("permissions")]
        [RequirePermission(Permissions.ManageRoles)]
        public async Task<IActionResult> GetAllPermissions()
        {
            var permissions = await _roleService.GetAllPermissionsAsync();
            return Ok(permissions);
        }
    }
}
