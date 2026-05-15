using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Application.DTOs.Projects;
using TaskManagementApi.Application.Interfaces;
using Microsoft.Extensions.Logging;
using TaskManagementApi.Web.Authorization;
using TaskManagementApi.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TaskManagementApi.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IAccessControlService _accessService;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(IProjectService projectService, IAccessControlService accessService, ILogger<ProjectsController> logger)
        {
            _projectService = projectService;
            _accessService = accessService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(userIdStr!);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<ActionResult<ProjectResponse>> CreateProject(CreateProjectRequest request)
        {
            try
            {
                var response = await _projectService.CreateProjectAsync(request);
                return CreatedAtAction(nameof(GetProject), new { id = response.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectResponse>>> GetProjects()
        {
            var userId = GetCurrentUserId();
            var isAdminOrPm = User.IsInRole("Admin") || User.IsInRole("ProjectManager");

            var projects = await _projectService.GetAllProjectsAsync(isAdminOrPm ? null : userId);
            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectResponse>> GetProject(int id)
        {
            var isAdminOrPm = User.IsInRole("Admin") || User.IsInRole("ProjectManager");
            if (!isAdminOrPm)
            {
                var userId = GetCurrentUserId();
                var isAssigned = await _projectService.IsUserAssignedToProjectAsync(userId, id);
                if (!isAssigned)
                {
                    return StatusCode(403, "You do not have access to this project");
                }
            }

            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null) return NotFound();
            return Ok(project);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> UpdateProject(int id, UpdateProjectRequest request)
        {
            await _projectService.UpdateProjectAsync(id, request);
            return NoContent();
        }

        [HttpPut("{id}/archive")]
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> ArchiveProject(int id)
        {
            await _projectService.ArchiveProjectAsync(id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            await _projectService.SoftDeleteProjectAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/teams")]
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> AssignTeamOrUser(int id, [FromQuery] int? teamId, [FromQuery] int? userId)
        {
            await _projectService.AssignTeamOrUserAsync(id, teamId, userId);
            return NoContent();
        }
    }
}
