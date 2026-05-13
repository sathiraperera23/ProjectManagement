using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Application.DTOs.Tickets;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Enums;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TaskManagementApi.Web.Authorization;
using TaskManagementApi.Domain.Constants;

namespace TaskManagementApi.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly IUserManagerFacade _userManager;

        public TicketsController(ITicketService ticketService, IUserManagerFacade userManager)
        {
            _ticketService = ticketService;
            _userManager = userManager;
        }

        private async Task<int> GetCurrentUserId()
        {
            var providerId = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (providerId == null) return 0;
            var user = await _userManager.FindByProviderIdAsync(providerId);
            return user?.Id ?? 0;
        }

        [HttpPost]
        [RequirePermission(Permissions.CreateTicket)]
        public async Task<ActionResult<TicketResponse>> CreateTicket(CreateTicketRequest request)
        {
            var response = await _ticketService.CreateTicketAsync(request, await GetCurrentUserId());
            return CreatedAtAction(nameof(GetTicket), new { id = response.Id }, response);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketResponse>>> GetTickets(
            [FromQuery] int? projectId, [FromQuery] int? productId, [FromQuery] int? subProjectId,
            [FromQuery] int? statusId, [FromQuery] TicketPriority? priority, [FromQuery] TicketCategory? category,
            [FromQuery] int? assigneeId, [FromQuery] int? teamId, [FromQuery] int? sprintId,
            [FromQuery] int? milestoneId, [FromQuery] string? label,
            [FromQuery] DateTime? dueDateFrom, [FromQuery] DateTime? dueDateTo)
        {
            var tickets = await _ticketService.GetAllTicketsAsync(
                projectId, productId, subProjectId, statusId, priority, category,
                assigneeId, teamId, sprintId, milestoneId, label, dueDateFrom, dueDateTo);
            return Ok(tickets);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TicketResponse>> GetTicket(int id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null) return NotFound();
            return Ok(ticket);
        }

        [HttpPut("{id}")]
        [RequirePermission(Permissions.EditAllTickets)]
        public async Task<IActionResult> UpdateTicket(int id, UpdateTicketRequest request)
        {
            await _ticketService.UpdateTicketAsync(id, request, await GetCurrentUserId());
            return NoContent();
        }

        [HttpPut("{id}/status")]
        [RequirePermission(Permissions.ChangeStatus)]
        public async Task<IActionResult> UpdateTicketStatus(int id, UpdateTicketStatusRequest request)
        {
            try
            {
                await _ticketService.UpdateTicketStatusAsync(id, request, await GetCurrentUserId());
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/assign")]
        [RequirePermission(Permissions.ReassignTicket)]
        public async Task<IActionResult> AssignTicket(int id, [FromBody] List<int> assigneeIds)
        {
            await _ticketService.AssignTicketAsync(id, assigneeIds, await GetCurrentUserId());
            return NoContent();
        }

        [HttpDelete("{id}")]
        [RequirePermission(Permissions.DeleteTicket)]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            await _ticketService.SoftDeleteTicketAsync(id);
            return NoContent();
        }

        [HttpPut("bulk/status")]
        [RequirePermission(Permissions.ChangeStatus)]
        public async Task<IActionResult> BulkUpdateStatus(BulkUpdateStatusRequest request)
        {
            await _ticketService.BulkUpdateStatusAsync(request, await GetCurrentUserId());
            return NoContent();
        }

        [HttpPut("bulk/assign")]
        [RequirePermission(Permissions.ReassignTicket)]
        public async Task<IActionResult> BulkAssign(BulkAssignRequest request)
        {
            await _ticketService.BulkAssignAsync(request, await GetCurrentUserId());
            return NoContent();
        }

        [HttpPut("bulk/priority")]
        [RequirePermission(Permissions.EditAllTickets)]
        public async Task<IActionResult> BulkUpdatePriority(BulkUpdatePriorityRequest request)
        {
            await _ticketService.BulkUpdatePriorityAsync(request, await GetCurrentUserId());
            return NoContent();
        }

        [HttpPost("{id}/links")]
        [RequirePermission(Permissions.EditAllTickets)]
        public async Task<IActionResult> LinkTickets(int id, LinkTicketRequest request)
        {
            await _ticketService.LinkTicketsAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id}/links/{linkId}")]
        [RequirePermission(Permissions.EditAllTickets)]
        public async Task<IActionResult> RemoveLink(int id, int linkId)
        {
            await _ticketService.RemoveLinkAsync(id, linkId);
            return NoContent();
        }

        [HttpGet("{id}/history")]
        public async Task<ActionResult<IEnumerable<TicketStatusHistoryResponse>>> GetTicketHistory(int id)
        {
            var history = await _ticketService.GetTicketHistoryAsync(id);
            return Ok(history);
        }
    }
}
