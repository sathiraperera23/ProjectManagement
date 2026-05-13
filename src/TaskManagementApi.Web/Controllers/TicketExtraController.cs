using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Application.DTOs.Tickets;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Application.Validators;
using TaskManagementApi.Domain.Constants;
using TaskManagementApi.Web.Authorization;
using System.Security.Claims;

namespace TaskManagementApi.Web.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class TicketExtraController : ControllerBase
    {
        private readonly ITicketExtraService _extraService;
        private readonly IUserManagerFacade _userManager;

        public TicketExtraController(ITicketExtraService extraService, IUserManagerFacade userManager)
        {
            _extraService = extraService;
            _userManager = userManager;
        }

        private async Task<int> GetCurrentUserId()
        {
            var providerId = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (providerId == null) return 0;
            var user = await _userManager.FindByProviderIdAsync(providerId);
            return user?.Id ?? 0;
        }

        // ── Comments ──────────────────────────────────────────

        [HttpGet("tickets/{ticketId}/comments")]
        public async Task<IActionResult> GetComments(int ticketId)
        {
            // For now assume all authorized users have team access for internal notes visibility
            var comments = await _extraService.GetTicketCommentsAsync(ticketId, true);
            return Ok(comments);
        }

        [HttpPost("tickets/{ticketId}/comments")]
        public async Task<IActionResult> PostComment(int ticketId, [FromBody] CreateCommentRequest request)
        {
            var validator = new CreateCommentRequestValidator();
            var result = await validator.ValidateAsync(request);
            if (!result.IsValid) return BadRequest(result.Errors.Select(e => e.ErrorMessage));

            var comment = await _extraService.PostCommentAsync(ticketId, request, await GetCurrentUserId());
            return Ok(comment);
        }

        [HttpPut("comments/{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentRequest request)
        {
            await _extraService.UpdateCommentAsync(id, request, await GetCurrentUserId());
            return NoContent();
        }

        [HttpDelete("comments/{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            await _extraService.DeleteCommentAsync(id, await GetCurrentUserId());
            return NoContent();
        }

        [HttpPost("comments/{id}/reactions")]
        public async Task<IActionResult> AddReaction(int id, [FromQuery] string emoji)
        {
            await _extraService.AddReactionAsync(id, emoji, await GetCurrentUserId());
            return NoContent();
        }

        [HttpDelete("comments/{id}/reactions/{emoji}")]
        public async Task<IActionResult> RemoveReaction(int id, string emoji)
        {
            await _extraService.RemoveReactionAsync(id, emoji, await GetCurrentUserId());
            return NoContent();
        }

        // ── Watchers ──────────────────────────────────────────

        [HttpPost("tickets/{ticketId}/watch")]
        public async Task<IActionResult> WatchTicket(int ticketId)
        {
            await _extraService.WatchTicketAsync(ticketId, await GetCurrentUserId());
            return NoContent();
        }

        [HttpDelete("tickets/{ticketId}/watch")]
        public async Task<IActionResult> UnwatchTicket(int ticketId)
        {
            await _extraService.UnwatchTicketAsync(ticketId, await GetCurrentUserId());
            return NoContent();
        }

        [HttpGet("tickets/{ticketId}/watchers")]
        public async Task<IActionResult> GetWatchers(int ticketId)
        {
            var watchers = await _extraService.GetWatchersAsync(ticketId);
            return Ok(watchers);
        }

        // ── Attachments ───────────────────────────────────────

        [HttpPost("tickets/{ticketId}/attachments")]
        public async Task<IActionResult> UploadAttachment(int ticketId, IFormFile file)
        {
            var attachment = await _extraService.UploadAttachmentAsync(ticketId, file, await GetCurrentUserId());
            return Ok(attachment);
        }

        [HttpPost("tickets/{ticketId}/attachments/link")]
        public async Task<IActionResult> LinkExternalAttachment(int ticketId, [FromBody] LinkExternalAttachmentRequest request)
        {
            var attachment = await _extraService.LinkExternalAttachmentAsync(ticketId, request, await GetCurrentUserId());
            return Ok(attachment);
        }

        [HttpGet("tickets/{ticketId}/attachments")]
        public async Task<IActionResult> GetAttachments(int ticketId)
        {
            var attachments = await _extraService.GetTicketAttachmentsAsync(ticketId);
            return Ok(attachments);
        }

        [HttpGet("attachments/{id}/download")]
        public async Task<IActionResult> DownloadAttachment(int id)
        {
            var attachment = await _extraService.GetAttachmentByIdAsync(id);
            if (attachment == null || attachment.DownloadUrl == null) return NotFound();

            var filePath = Path.Combine("wwwroot", attachment.DownloadUrl.TrimStart('/'));
            if (!System.IO.File.Exists(filePath)) return NotFound("File not found on disk");

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, attachment.ContentType ?? "application/octet-stream", attachment.FileName);
        }

        [HttpDelete("tickets/{ticketId}/attachments/{id}")]
        public async Task<IActionResult> DeleteAttachment(int ticketId, int id)
        {
            await _extraService.DeleteAttachmentAsync(ticketId, id, await GetCurrentUserId());
            return NoContent();
        }

        // ── Daily Updates ──────────────────────────────────────

        [HttpPost("daily-updates")]
        public async Task<IActionResult> PostDailyUpdate([FromBody] CreateDailyUpdateRequest request)
        {
            var validator = new CreateDailyUpdateRequestValidator();
            var result = await validator.ValidateAsync(request);
            if (!result.IsValid) return BadRequest(result.Errors.Select(e => e.ErrorMessage));

            var update = await _extraService.PostDailyUpdateAsync(request, await GetCurrentUserId());
            return Ok(update);
        }

        [HttpPut("daily-updates/{id}")]
        public async Task<IActionResult> UpdateDailyUpdate(int id, [FromBody] CreateDailyUpdateRequest request)
        {
            await _extraService.UpdateDailyUpdateAsync(id, request, await GetCurrentUserId());
            return NoContent();
        }

        [HttpGet("daily-updates/mine")]
        public async Task<IActionResult> GetMyDailyUpdates()
        {
            var updates = await _extraService.GetUserDailyUpdatesAsync(await GetCurrentUserId());
            return Ok(updates);
        }

        [HttpGet("projects/{projectId}/daily-updates")]
        public async Task<IActionResult> GetProjectDailyUpdates(int projectId, [FromQuery] int? userId, [FromQuery] DateTime? date)
        {
            var updates = await _extraService.GetProjectDailyUpdatesAsync(projectId, userId, date);
            return Ok(updates);
        }

        [HttpPost("daily-updates/{id}/tickets/{ticketId}")]
        public async Task<IActionResult> LinkUpdateToTicket(int id, int ticketId)
        {
            await _extraService.LinkUpdateToTicketAsync(id, ticketId, await GetCurrentUserId());
            return NoContent();
        }
    }
}
