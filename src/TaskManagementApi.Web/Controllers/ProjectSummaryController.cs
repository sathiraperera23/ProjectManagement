using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Application.DTOs.Projects;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Constants;
using TaskManagementApi.Web.Authorization;
using System.Security.Claims;

namespace TaskManagementApi.Web.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/summary")]
    [Authorize]
    public class ProjectSummaryController : ControllerBase
    {
        private readonly IProjectSummaryService _summaryService;
        private readonly IAccessControlService _accessService;

        public ProjectSummaryController(IProjectSummaryService summaryService, IAccessControlService accessService)
        {
            _summaryService = summaryService;
            _accessService = accessService;
        }

        private Task<int> GetCurrentUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            return Task.FromResult(int.TryParse(userIdStr, out var id) ? id : 0);
        }

        [HttpGet]
        public async Task<IActionResult> GetSummary(int projectId)
        {
            // RBAC Check
            var access = await _accessService.GetAccessLevelAsync(await GetCurrentUserId(), Domain.Entities.AccessComponentType.Project, projectId);
            if (access == Domain.Entities.AccessLevel.NoAccess) return Forbid();

            var summary = await _summaryService.GetProjectSummaryAsync(projectId);
            return Ok(summary);
        }

        [HttpPut("/api/projects/{projectId}/wip-limits")]
        [RequirePermission(Permissions.EditProject)]
        public async Task<IActionResult> SetWipLimit(int projectId, [FromBody] SetWipLimitRequest request)
        {
            await _summaryService.SetWipLimitAsync(projectId, request);
            return NoContent();
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportSummary(int projectId)
        {
            var pdf = await _summaryService.ExportSummaryPdfAsync(projectId);
            return File(pdf, "application/pdf", $"ProjectSummary_{projectId}.pdf");
        }
    }

    [ApiController]
    [Route("api")]
    [Authorize]
    public class AccessControlController : ControllerBase
    {
        private readonly IAccessControlService _accessService;

        public AccessControlController(IAccessControlService accessService)
        {
            _accessService = accessService;
        }

        private Task<int> GetCurrentUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            return Task.FromResult(int.TryParse(userIdStr, out var id) ? id : 0);
        }

        [HttpGet("projects/{projectId}/access-rules")]
        [RequirePermission(Permissions.ManageAccessRules)]
        public async Task<IActionResult> GetRules(int projectId)
        {
            var rules = await _accessService.GetProjectRulesAsync(projectId);
            return Ok(rules);
        }

        [HttpPost("projects/{projectId}/access-rules")]
        [RequirePermission(Permissions.ManageAccessRules)]
        public async Task<IActionResult> CreateRule(int projectId, [FromBody] CreateAccessRuleRequest request)
        {
            var rule = await _accessService.CreateRuleAsync(projectId, request, await GetCurrentUserId());
            return Ok(rule);
        }

        [HttpDelete("access-rules/{id}")]
        [RequirePermission(Permissions.ManageAccessRules)]
        public async Task<IActionResult> DeleteRule(int id)
        {
            await _accessService.DeleteRuleAsync(id);
            return NoContent();
        }

        [HttpPost("access-requests")]
        public async Task<IActionResult> SubmitRequest([FromBody] CreateAccessRuleRequest request, [FromQuery] string note)
        {
            await _accessService.SubmitAccessRequestAsync(await GetCurrentUserId(), request, note);
            return Ok();
        }

        [HttpGet("projects/{projectId}/access-requests")]
        [RequirePermission(Permissions.ManageAccessRules)]
        public async Task<IActionResult> GetRequests(int projectId)
        {
            var items = await _accessService.GetPendingRequestsAsync(projectId);
            return Ok(items);
        }

        [HttpPut("access-requests/{id}/approve")]
        [RequirePermission(Permissions.ManageAccessRules)]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            await _accessService.ApproveRequestAsync(id, await GetCurrentUserId());
            return NoContent();
        }
    }
}
