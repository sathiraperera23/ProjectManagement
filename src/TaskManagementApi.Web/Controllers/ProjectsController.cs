using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Application.DTOs.Projects;
using TaskManagementApi.Application.Interfaces;

namespace TaskManagementApi.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IAccessControlService _accessService;
        private readonly IUserManagerFacade _userManager;

        public ProjectsController(IProjectService projectService, IAccessControlService accessService, IUserManagerFacade userManager)
        {
            _projectService = projectService;
            _accessService = accessService;
            _userManager = userManager;
        }

        private async Task<int> GetCurrentUserId()
        {
            var providerId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (providerId == null) return 0;
            var user = await _userManager.FindByProviderIdAsync(providerId);
            return user?.Id ?? 0;
        }

        [HttpPost]
        public async Task<ActionResult<ProjectResponse>> CreateProject(CreateProjectRequest request)
        {
            // Requires CREATE_PROJECT permission (to be added)
            var response = await _projectService.CreateProjectAsync(request);
            return CreatedAtAction(nameof(GetProject), new { id = response.Id }, response);
        }

        [HttpGet]
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

            var project = _projectService.GetProjectByIdAsync(id);
            if (project == null) return NotFound();
            return Ok(project);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, UpdateProjectRequest request)
        {
            // Requires EDIT_PROJECT permission
            await _projectService.UpdateProjectAsync(id, request);
            return NoContent();
        }

        [HttpPut("{id}/archive")]
        public async Task<IActionResult> ArchiveProject(int id)
        {
            // Requires ARCHIVE_PROJECT permission
            await _projectService.ArchiveProjectAsync(id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            // Requires DELETE_PROJECT permission
            await _projectService.SoftDeleteProjectAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/teams")]
        public async Task<IActionResult> AssignTeamOrUser(int id, [FromQuery] int? teamId, [FromQuery] int? userId)
        {
            await _projectService.AssignTeamOrUserAsync(id, teamId, userId);
            return NoContent();
        }
    }
}
