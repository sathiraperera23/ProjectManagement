using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Application.DTOs.Backlog;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Application.Validators;
using TaskManagementApi.Domain.Constants;
using TaskManagementApi.Web.Authorization;
using Microsoft.AspNetCore.Http;

namespace TaskManagementApi.Web.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class BacklogController : ControllerBase
    {
        private readonly IBacklogService _backlogService;

        public BacklogController(IBacklogService backlogService)
        {
            _backlogService = backlogService;
        }

        private int GetCurrentUserId() => int.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // ── Project-level backlog ──────────────────────────────

        [HttpGet("projects/{projectId}/backlog")]
        [RequirePermission(Permissions.ViewAllProjects)]
        public async Task<IActionResult> GetProjectBacklog(int projectId, [FromQuery] BacklogFilterRequest filter)
        {
            var items = await _backlogService.GetProjectBacklogAsync(projectId, filter);
            return Ok(items);
        }

        // ── Product-level backlog ──────────────────────────────

        [HttpGet("products/{productId}/backlog")]
        [RequirePermission(Permissions.ViewProductBacklog)]
        public async Task<IActionResult> GetProductBacklog(int productId, [FromQuery] BacklogFilterRequest filter)
        {
            var items = await _backlogService.GetProductBacklogAsync(productId, filter);
            return Ok(items);
        }

        // ── Backlog item CRUD ──────────────────────────────────

        [HttpGet("backlog/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _backlogService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost("backlog")]
        [RequirePermission(Permissions.ManageBrds)]
        public async Task<IActionResult> Create([FromBody] CreateBacklogItemRequest request)
        {
            var validator = new CreateBacklogItemRequestValidator();
            var result = await validator.ValidateAsync(request);
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(e => e.ErrorMessage));

            var item = await _backlogService.CreateAsync(request, GetCurrentUserId());
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPut("backlog/{id}")]
        [RequirePermission(Permissions.ManageBrds)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBacklogItemRequest request)
        {
            var validator = new UpdateBacklogItemRequestValidator();
            var result = await validator.ValidateAsync(request);
            if (!result.IsValid)
                return BadRequest(result.Errors.Select(e => e.ErrorMessage));

            var item = await _backlogService.UpdateAsync(id, request, GetCurrentUserId());
            return Ok(item);
        }

        [HttpDelete("backlog/{id}")]
        [RequirePermission(Permissions.ManageBrds)]
        public async Task<IActionResult> Delete(int id)
        {
            await _backlogService.DeleteAsync(id, GetCurrentUserId());
            return NoContent();
        }

        // ── Ordering ───────────────────────────────────────────

        [HttpPut("projects/{projectId}/backlog/reorder")]
        [RequirePermission(Permissions.ManageBrds)]
        public async Task<IActionResult> Reorder(int projectId, [FromBody] ReorderBacklogRequest request)
        {
            await _backlogService.ReorderAsync(projectId, request);
            return NoContent();
        }

        // ── Version history ────────────────────────────────────

        [HttpGet("backlog/{id}/versions")]
        public async Task<IActionResult> GetVersionHistory(int id)
        {
            var versions = await _backlogService.GetVersionHistoryAsync(id);
            return Ok(versions);
        }

        [HttpGet("backlog/{id}/versions/{versionNumber}")]
        public async Task<IActionResult> GetVersion(int id, int versionNumber)
        {
            var version = await _backlogService.GetVersionAsync(id, versionNumber);
            if (version == null) return NotFound();
            return Ok(version);
        }

        // ── Attachments ────────────────────────────────────────

        [HttpPost("backlog/{id}/attachments")]
        [RequirePermission(Permissions.ManageBrds)]
        public async Task<IActionResult> AddAttachment(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file provided");

            var attachment = await _backlogService.AddAttachmentAsync(id, file, GetCurrentUserId());
            return Ok(attachment);
        }

        [HttpDelete("backlog/{id}/attachments/{attachmentId}")]
        [RequirePermission(Permissions.ManageBrds)]
        public async Task<IActionResult> DeleteAttachment(int id, int attachmentId)
        {
            await _backlogService.DeleteAttachmentAsync(id, attachmentId);
            return NoContent();
        }

        [HttpGet("backlog/attachments/{attachmentId}/download")]
        public async Task<IActionResult> DownloadAttachment(int attachmentId)
        {
            var attachment = await _backlogService.GetAttachmentAsync(attachmentId);
            if (attachment == null) return NotFound();

            // The DownloadUrl is now a relative path like /uploads/backlog/1/file.ext
            // We should use IWebHostEnvironment to find the file or serve it via Redirect if static files middleware is enabled.
            // For this controller action, we serve it directly.
            var filePath = Path.Combine("wwwroot", attachment.DownloadUrl.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found on disk");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, attachment.ContentType, attachment.FileName);
        }

        // ── Traceability ───────────────────────────────────────

        [HttpPost("backlog/{id}/tickets/{ticketId}")]
        [RequirePermission(Permissions.ManageBrds)]
        public async Task<IActionResult> LinkToTicket(int id, int ticketId)
        {
            await _backlogService.LinkToTicketAsync(id, ticketId, GetCurrentUserId());
            return NoContent();
        }

        [HttpDelete("backlog/{id}/tickets/{ticketId}")]
        [RequirePermission(Permissions.ManageBrds)]
        public async Task<IActionResult> UnlinkFromTicket(int id, int ticketId)
        {
            await _backlogService.UnlinkFromTicketAsync(id, ticketId);
            return NoContent();
        }

        [HttpGet("tickets/{ticketId}/backlog-items")]
        public async Task<IActionResult> GetLinkedItemsForTicket(int ticketId)
        {
            var items = await _backlogService.GetLinkedItemsForTicketAsync(ticketId);
            return Ok(items);
        }

        // ── Approval workflow ──────────────────────────────────

        [HttpPost("backlog/{id}/submit-for-approval")]
        [RequirePermission(Permissions.ManageBrds)]
        public async Task<IActionResult> SubmitForApproval(int id)
        {
            var approval = await _backlogService.SubmitForApprovalAsync(id, GetCurrentUserId());
            return Ok(approval);
        }

        [HttpPost("backlog/{id}/approvals/{approvalRequestId}/approve")]
        [RequirePermission(Permissions.ApproveRequirements)]
        public async Task<IActionResult> Approve(int id, int approvalRequestId, [FromBody] ApprovalActionRequest request)
        {
            var approval = await _backlogService.ApproveAsync(id, approvalRequestId, request.Note, GetCurrentUserId());
            return Ok(approval);
        }

        [HttpPost("backlog/{id}/approvals/{approvalRequestId}/reject")]
        [RequirePermission(Permissions.ApproveRequirements)]
        public async Task<IActionResult> Reject(int id, int approvalRequestId, [FromBody] ApprovalActionRequest request)
        {
            if (string.IsNullOrEmpty(request.Note))
                return BadRequest("A rejection reason is required");

            var approval = await _backlogService.RejectAsync(id, approvalRequestId, request.Note, GetCurrentUserId());
            return Ok(approval);
        }

        [HttpPost("backlog/{id}/approvals/{approvalRequestId}/request-changes")]
        [RequirePermission(Permissions.ApproveRequirements)]
        public async Task<IActionResult> RequestChanges(int id, int approvalRequestId, [FromBody] ApprovalActionRequest request)
        {
            if (string.IsNullOrEmpty(request.Note))
                return BadRequest("A note is required when requesting changes");

            var approval = await _backlogService.RequestChangesAsync(id, approvalRequestId, request.Note, GetCurrentUserId());
            return Ok(approval);
        }
    }

    public class ApprovalActionRequest
    {
        public string? Note { get; set; }
    }
}
