using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Application.DTOs.CustomerBugs;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Constants;
using TaskManagementApi.Web.Authorization;
using System.Security.Claims;

namespace TaskManagementApi.Web.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class CustomerBugController : ControllerBase
    {
        private readonly ICustomerBugService _bugService;
        private readonly IBugReportTemplateService _templateService;
        private readonly IUserManagerFacade _userManager;

        public CustomerBugController(ICustomerBugService bugService, IBugReportTemplateService templateService, IUserManagerFacade userManager)
        {
            _bugService = bugService;
            _templateService = templateService;
            _userManager = userManager;
        }

        private async Task<int> GetCurrentUserId()
        {
            var providerId = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (providerId == null) return 0;
            var user = await _userManager.FindByProviderIdAsync(providerId);
            return user?.Id ?? 0;
        }

        [HttpGet("projects/{projectId}/bug-submissions")]
        public async Task<IActionResult> GetSubmissions(int projectId)
        {
            var submissions = await _bugService.GetSubmissionsAsync(projectId);
            return Ok(submissions);
        }

        [HttpGet("projects/{projectId}/bug-approval-queue")]
        public async Task<IActionResult> GetApprovalQueue(int projectId)
        {
            var queue = await _bugService.GetApprovalQueueAsync(projectId);
            return Ok(queue);
        }

        [HttpPost("tickets/{ticketId}/approve")]
        [RequirePermission(Permissions.ApproveTickets)]
        public async Task<IActionResult> ApproveBug(int ticketId, [FromBody] BugApprovalRequest request)
        {
            await _bugService.ApproveBugAsync(ticketId, request, await GetCurrentUserId());
            return NoContent();
        }

        [HttpPost("tickets/{ticketId}/reject")]
        [RequirePermission(Permissions.ApproveTickets)]
        public async Task<IActionResult> RejectBug(int ticketId, [FromBody] BugRejectionRequest request)
        {
            await _bugService.RejectBugAsync(ticketId, request, await GetCurrentUserId());
            return NoContent();
        }

        [HttpPost("tickets/{ticketId}/request-more-info")]
        public async Task<IActionResult> RequestMoreInfo(int ticketId, [FromBody] string message)
        {
            await _bugService.RequestMoreInfoAsync(ticketId, message, await GetCurrentUserId());
            return NoContent();
        }

        [HttpPost("inbound-email")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleInboundEmail([FromBody] BugInboundEmailRequest request)
        {
            // Webhook source validation should be added here
            await _bugService.HandleInboundEmailAsync(request);
            return NoContent();
        }

        [HttpGet("projects/{projectId}/bug-sla")]
        public async Task<IActionResult> GetSla(int projectId)
        {
            var sla = await _bugService.GetSlaAsync(projectId);
            if (sla == null) return NotFound();
            return Ok(sla);
        }

        [HttpPut("projects/{projectId}/bug-sla")]
        [RequirePermission(Permissions.ManageAccessRules)]
        public async Task<IActionResult> UpdateSla(int projectId, [FromBody] UpdateBugSlaRequest request)
        {
            await _bugService.UpdateSlaAsync(projectId, request);
            return NoContent();
        }

        [HttpGet("projects/{projectId}/bug-report-template")]
        public async Task<IActionResult> GetTemplate(int projectId)
        {
            var template = await _templateService.GetTemplateAsync(projectId);
            var bytes = System.Text.Encoding.UTF8.GetBytes(template);
            return File(bytes, "text/plain", "BugReportTemplate.txt");
        }

        [HttpGet("projects/{projectId}/bug-report-template/preview")]
        public async Task<IActionResult> GetTemplatePreview(int projectId)
        {
            var template = await _templateService.GetTemplateAsync(projectId);
            return Ok(new { template });
        }

        [HttpPut("projects/{projectId}/bug-report-template")]
        [RequirePermission(Permissions.ManageAccessRules)]
        public async Task<IActionResult> UpdateTemplateCustomText(int projectId, [FromBody] string customText)
        {
            await _templateService.UpdateCustomTextAsync(projectId, customText, await GetCurrentUserId());
            return NoContent();
        }
    }
}
