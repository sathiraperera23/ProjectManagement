using TaskManagementApi.Application.DTOs.Projects;

namespace TaskManagementApi.Application.Interfaces
{
    public interface IProjectService
    {
        Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request);
        Task<IEnumerable<ProjectResponse>> GetAllProjectsAsync();
        Task<ProjectResponse?> GetProjectByIdAsync(int id);
        Task UpdateProjectAsync(int id, UpdateProjectRequest request);
        Task ArchiveProjectAsync(int id);
        Task SoftDeleteProjectAsync(int id);
        Task AssignTeamOrUserAsync(int id, int? teamId, int? userId);
    }
}
