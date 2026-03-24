using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Application.DTOs.Tickets;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Enums;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace TaskManagementApi.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        private int GetCurrentUserId() => int.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpPost]
        public async Task<ActionResult<TicketResponse>> CreateTicket(CreateTicketRequest request)
        {
            var response = await _ticketService.CreateTicketAsync(request, GetCurrentUserId());
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
        public async Task<IActionResult> UpdateTicket(int id, UpdateTicketRequest request)
        {
            await _ticketService.UpdateTicketAsync(id, request, GetCurrentUserId());
            return NoContent();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateTicketStatus(int id, UpdateTicketStatusRequest request)
        {
            try
            {
                await _ticketService.UpdateTicketStatusAsync(id, request, GetCurrentUserId());
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/assign")]
        public async Task<IActionResult> AssignTicket(int id, [FromBody] List<int> assigneeIds)
        {
            await _ticketService.AssignTicketAsync(id, assigneeIds, GetCurrentUserId());
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            await _ticketService.SoftDeleteTicketAsync(id);
            return NoContent();
        }

        [HttpPut("bulk/status")]
        public async Task<IActionResult> BulkUpdateStatus(BulkUpdateStatusRequest request)
        {
            await _ticketService.BulkUpdateStatusAsync(request, GetCurrentUserId());
            return NoContent();
        }

        [HttpPut("bulk/assign")]
        public async Task<IActionResult> BulkAssign(BulkAssignRequest request)
        {
            await _ticketService.BulkAssignAsync(request, GetCurrentUserId());
            return NoContent();
        }

        [HttpPut("bulk/priority")]
        public async Task<IActionResult> BulkUpdatePriority(BulkUpdatePriorityRequest request)
        {
            await _ticketService.BulkUpdatePriorityAsync(request, GetCurrentUserId());
            return NoContent();
        }

        [HttpPost("{id}/links")]
        public async Task<IActionResult> LinkTickets(int id, LinkTicketRequest request)
        {
            await _ticketService.LinkTicketsAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id}/links/{linkId}")]
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
