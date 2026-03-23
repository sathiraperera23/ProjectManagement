using TaskManagementApi.Application.DTOs.SubProjects;

namespace TaskManagementApi.Application.Interfaces
{
    public interface ISubProjectService
    {
        Task<SubProjectResponse> CreateSubProjectAsync(int productId, CreateSubProjectRequest request);
        Task<IEnumerable<SubProjectResponse>> GetSubProjectsByProductIdAsync(int productId);
        Task<SubProjectResponse?> GetSubProjectByIdAsync(int id);
        Task UpdateSubProjectAsync(int id, UpdateSubProjectRequest request);
        Task SoftDeleteSubProjectAsync(int id);
        Task AssignTeamToSubProjectAsync(int id, int teamId);
        Task<SubProjectProgressResponse> GetSubProjectProgressAsync(int id);
    }
}
