using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Application.DTOs.SubProjects;
using TaskManagementApi.Application.Interfaces;

namespace TaskManagementApi.Web.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/products/{productId}/[controller]")]
    public class SubProjectsController : ControllerBase
    {
        private readonly ISubProjectService _subProjectService;

        public SubProjectsController(ISubProjectService subProjectService)
        {
            _subProjectService = subProjectService;
        }

        [HttpPost]
        public async Task<ActionResult<SubProjectResponse>> CreateSubProject(int productId, CreateSubProjectRequest request)
        {
            var response = await _subProjectService.CreateSubProjectAsync(productId, request);
            return CreatedAtAction(nameof(GetSubProject), new { productId, id = response.Id }, response);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubProjectResponse>>> GetSubProjects(int productId)
        {
            var subProjects = await _subProjectService.GetSubProjectsByProductIdAsync(productId);
            return Ok(subProjects);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SubProjectResponse>> GetSubProject(int productId, int id)
        {
            var subProject = await _subProjectService.GetSubProjectByIdAsync(id);
            if (subProject == null) return NotFound();
            return Ok(subProject);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubProject(int productId, int id, UpdateSubProjectRequest request)
        {
            await _subProjectService.UpdateSubProjectAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubProject(int productId, int id)
        {
            await _subProjectService.SoftDeleteSubProjectAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/teams")]
        public async Task<IActionResult> AssignTeamToSubProject(int id, [FromQuery] int teamId)
        {
            await _subProjectService.AssignTeamToSubProjectAsync(id, teamId);
            return NoContent();
        }

        [HttpGet("{id}/progress")]
        public async Task<ActionResult<SubProjectProgressResponse>> GetSubProjectProgress(int id)
        {
            var progress = await _subProjectService.GetSubProjectProgressAsync(id);
            return Ok(progress);
        }
    }
}
