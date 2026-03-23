using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Application.DTOs.Tickets;
using TaskManagementApi.Application.Interfaces;

namespace TaskManagementApi.Web.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/statuses")]
    public class TicketStatusesController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketStatusesController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketStatusResponse>>> GetStatuses(int projectId)
        {
            var statuses = await _ticketService.GetStatusesByProjectIdAsync(projectId);
            return Ok(statuses);
        }

        [HttpPost]
        public async Task<ActionResult<TicketStatusResponse>> CreateStatus(int projectId, CreateTicketStatusRequest request)
        {
            var response = await _ticketService.CreateStatusAsync(projectId, request);
            return CreatedAtAction(nameof(GetStatuses), new { projectId }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStatus(int projectId, int id, CreateTicketStatusRequest request)
        {
            await _ticketService.UpdateStatusAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStatus(int projectId, int id)
        {
            await _ticketService.DeleteStatusAsync(id);
            return NoContent();
        }
    }
}
