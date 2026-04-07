using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Application.DTOs.Sprints;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Application.Validators;
using TaskManagementApi.Domain.Constants;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Web.Authorization;
using System.Security.Claims;

namespace TaskManagementApi.Web.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/sprints")]
    [Authorize]
    public class SprintController : ControllerBase
    {
        private readonly ISprintService _sprintService;
        private readonly IUserManagerFacade _userManager;

        public SprintController(ISprintService sprintService, IUserManagerFacade userManager)
        {
            _sprintService = sprintService;
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
        [RequirePermission(Permissions.CreateSprint)]
        public async Task<IActionResult> Create(int projectId, [FromBody] CreateSprintRequest request)
        {
            var validator = new CreateSprintRequestValidator();
            var result = await validator.ValidateAsync(request);
            if (!result.IsValid) return BadRequest(result.Errors.Select(e => e.ErrorMessage));

            var sprint = await _sprintService.CreateAsync(projectId, request, await GetCurrentUserId());
            return CreatedAtAction(nameof(GetById), new { projectId, id = sprint.Id }, sprint);
        }

        [HttpGet]
        public async Task<IActionResult> GetProjectSprints(int projectId)
        {
            var sprints = await _sprintService.GetProjectSprintsAsync(projectId);
            return Ok(sprints);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveSprint(int projectId, [FromQuery] int? subProjectId)
        {
            var sprint = await _sprintService.GetActiveSprintAsync(projectId, subProjectId);
            if (sprint == null) return NotFound();
            return Ok(sprint);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int projectId, int id)
        {
            var sprint = await _sprintService.GetByIdAsync(id);
            if (sprint == null) return NotFound();
            return Ok(sprint);
        }

        [HttpPut("{id}")]
        [RequirePermission(Permissions.CreateSprint)]
        public async Task<IActionResult> Update(int projectId, int id, [FromBody] UpdateSprintRequest request)
        {
            var validator = new UpdateSprintRequestValidator();
            var result = await validator.ValidateAsync(request);
            if (!result.IsValid) return BadRequest(result.Errors.Select(e => e.ErrorMessage));

            await _sprintService.UpdateAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [RequirePermission(Permissions.CreateSprint)]
        public async Task<IActionResult> Delete(int projectId, int id)
        {
            await _sprintService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/activate")]
        [RequirePermission(Permissions.CreateSprint)]
        public async Task<IActionResult> Activate(int projectId, int id)
        {
            await _sprintService.ActivateAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/close")]
        [RequirePermission(Permissions.CloseSprint)]
        public async Task<IActionResult> Close(int projectId, int id, [FromBody] CloseSprintRequest request)
        {
            var validator = new CloseSprintRequestValidator();
            var result = await validator.ValidateAsync(request);
            if (!result.IsValid) return BadRequest(result.Errors.Select(e => e.ErrorMessage));

            await _sprintService.CloseAsync(id, request, await GetCurrentUserId());
            return NoContent();
        }

        [HttpPost("{id}/tickets/{ticketId}")]
        [RequirePermission(Permissions.MoveTicketsToSprint)]
        public async Task<IActionResult> AddTicket(int projectId, int id, int ticketId, [FromQuery] string? reason)
        {
            await _sprintService.AddTicketToSprintAsync(id, ticketId, reason, await GetCurrentUserId());
            return NoContent();
        }

        [HttpDelete("{id}/tickets/{ticketId}")]
        public async Task<IActionResult> RemoveTicket(int projectId, int id, int ticketId, [FromQuery] string? reason)
        {
            await _sprintService.RemoveTicketFromSprintAsync(id, ticketId, reason, await GetCurrentUserId());
            return NoContent();
        }

        [HttpGet("{id}/tickets")]
        public async Task<IActionResult> GetTickets(int projectId, int id)
        {
            var tickets = await _sprintService.GetSprintTicketsAsync(id);
            return Ok(tickets);
        }

        [HttpGet("{id}/capacity")]
        public async Task<IActionResult> GetCapacity(int projectId, int id)
        {
            var capacities = await _sprintService.GetMemberCapacitiesAsync(id);
            return Ok(capacities);
        }

        [HttpPut("{id}/capacity")]
        public async Task<IActionResult> SetCapacity(int projectId, int id, [FromBody] SetSprintMemberCapacityRequest request)
        {
            await _sprintService.SetMemberCapacitiesAsync(id, request);
            return NoContent();
        }

        [HttpGet("{id}/summary")]
        public async Task<IActionResult> GetSummary(int projectId, int id)
        {
            var summary = await _sprintService.GetSummaryAsync(id);
            return Ok(summary);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory(int projectId)
        {
            var history = await _sprintService.GetHistoryAsync(projectId);
            return Ok(history);
        }

        [HttpGet("{id}/scope-changes")]
        public async Task<IActionResult> GetScopeChanges(int projectId, int id)
        {
            var changes = await _sprintService.GetScopeChangesAsync(id);
            return Ok(changes);
        }

        [HttpGet("velocity")]
        public async Task<IActionResult> GetVelocity(int projectId)
        {
            var velocity = await _sprintService.GetVelocityAsync(projectId);
            return Ok(velocity);
        }
    }
}
