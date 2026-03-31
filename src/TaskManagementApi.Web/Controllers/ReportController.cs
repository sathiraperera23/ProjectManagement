using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Application.DTOs.Reports;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Constants;
using TaskManagementApi.Web.Authorization;
using System.Security.Claims;

namespace TaskManagementApi.Web.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly IAccessControlService _accessService;
        private readonly IUserManagerFacade _userManager;

        public ReportController(IReportService reportService, IAccessControlService accessService, IUserManagerFacade userManager)
        {
            _reportService = reportService;
            _accessService = accessService;
            _userManager = userManager;
        }

        private async Task<int> GetCurrentUserId()
        {
            var providerId = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (providerId == null) return 0;
            var user = await _userManager.FindByProviderIdAsync(providerId);
            return user?.Id ?? 0;
        }

        // ── Time Logs ─────────────────────────────────────────

        [HttpPost("tickets/{ticketId}/time-logs")]
        public async Task<IActionResult> LogTime(int ticketId, [FromBody] CreateTimeLogRequest request)
        {
            var log = await _reportService.LogTimeAsync(ticketId, request, await GetCurrentUserId());
            return Ok(log);
        }

        [HttpGet("tickets/{ticketId}/time-logs")]
        public async Task<IActionResult> GetTimeLogs(int ticketId)
        {
            var logs = await _reportService.GetTicketTimeLogsAsync(ticketId);
            return Ok(logs);
        }

        [HttpDelete("time-logs/{id}")]
        public async Task<IActionResult> DeleteTimeLog(int id)
        {
            await _reportService.DeleteTimeLogAsync(id, await GetCurrentUserId());
            return NoContent();
        }

        // ── Budget ────────────────────────────────────────────

        [HttpPost("projects/{projectId}/budget")]
        [RequirePermission(Permissions.ViewBudgetData)]
        public async Task<IActionResult> SetBudget(int projectId, [FromBody] SetProjectBudgetRequest request)
        {
            await _reportService.SetBudgetAsync(projectId, request, await GetCurrentUserId());
            return NoContent();
        }

        [HttpGet("projects/{projectId}/budget")]
        [RequirePermission(Permissions.ViewBudgetData)]
        public async Task<IActionResult> GetBudget(int projectId)
        {
            var budget = await _reportService.GetBudgetAsync(projectId);
            if (budget == null) return NotFound();
            return Ok(budget);
        }

        // ── Reports ───────────────────────────────────────────

        [HttpGet("projects/{projectId}/reports/rtm")]
        public async Task<IActionResult> GetRtm(int projectId, [FromQuery] int? subProjectId, [FromQuery] int? productId)
        {
            // RBAC Check
            var access = await _accessService.GetAccessLevelAsync(await GetCurrentUserId(), Domain.Entities.AccessComponentType.Report, projectId);
            if (access == Domain.Entities.AccessLevel.NoAccess) return Forbid();

            var report = await _reportService.GetRtmReportAsync(projectId, subProjectId, productId);
            return Ok(report);
        }

        [HttpGet("projects/{projectId}/reports/dependency-matrix")]
        public async Task<IActionResult> GetDependencyMatrix(int projectId, [FromQuery] int? subProjectId, [FromQuery] int? sprintId)
        {
            var report = await _reportService.GetDependencyMatrixAsync(projectId, subProjectId, sprintId);
            return Ok(report);
        }

        [HttpGet("projects/{projectId}/reports/costing")]
        [RequirePermission(Permissions.ViewCostingData)]
        public async Task<IActionResult> GetCosting(int projectId, [FromQuery] int? subProjectId, [FromQuery] int? productId)
        {
            var report = await _reportService.GetCostingReportAsync(projectId, subProjectId, productId);
            return Ok(report);
        }

        [HttpGet("projects/{projectId}/reports/delays")]
        public async Task<IActionResult> GetDelays(int projectId)
        {
            var report = await _reportService.GetDelayReportAsync(projectId);
            return Ok(report);
        }
    }
}
