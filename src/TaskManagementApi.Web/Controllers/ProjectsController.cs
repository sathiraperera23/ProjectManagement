using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Application.DTOs.Projects;
using TaskManagementApi.Application.Interfaces;
using Microsoft.Extensions.Logging;
using TaskManagementApi.Web.Authorization;
using TaskManagementApi.Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace TaskManagementApi.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IAccessControlService _accessService;
        private readonly IUserManagerFacade _userManager;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(IProjectService projectService, IAccessControlService accessService, IUserManagerFacade userManager, ILogger<ProjectsController> logger)
        {
            _projectService = projectService;
            _accessService = accessService;
            _userManager = userManager;
            _logger = logger;
        }

        private async Task<int> GetCurrentUserId()
        {
            var providerId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (providerId == null) return 0;
            var user = await _userManager.FindByProviderIdAsync(providerId);
            return user?.Id ?? 0;
        }

        [HttpPost]
        [RequirePermission(Permissions.CreateProject)]
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
        [RequirePermission(Permissions.ViewAllProjects)]
        public async Task<ActionResult<IEnumerable<ProjectResponse>>> GetProjects()
        {
            var projects = await _projectService.GetAllProjectsAsync();
            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectResponse>> GetProject(int id)
        {
            // RBAC Check
            var access = await _accessService.GetAccessLevelAsync(await GetCurrentUserId(), Domain.Entities.AccessComponentType.Project, id);
            if (access == Domain.Entities.AccessLevel.NoAccess) return Forbid();

            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null) return NotFound();
            return Ok(project);
        }

        [HttpPut("{id}")]
        [RequirePermission(Permissions.EditProject)]
        public async Task<IActionResult> UpdateProject(int id, UpdateProjectRequest request)
        {
            await _projectService.UpdateProjectAsync(id, request);
            return NoContent();
        }

        [HttpPut("{id}/archive")]
        [RequirePermission(Permissions.ArchiveProject)]
        public async Task<IActionResult> ArchiveProject(int id)
        {
            await _projectService.ArchiveProjectAsync(id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [RequirePermission(Permissions.DeleteProject)]
        public async Task<IActionResult> DeleteProject(int id)
        {
            await _projectService.SoftDeleteProjectAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/teams")]
        [RequirePermission(Permissions.EditProject)]
        public async Task<IActionResult> AssignTeamOrUser(int id, [FromQuery] int? teamId, [FromQuery] int? userId)
        {
            await _projectService.AssignTeamOrUserAsync(id, teamId, userId);
            return NoContent();
        }
    }
}
