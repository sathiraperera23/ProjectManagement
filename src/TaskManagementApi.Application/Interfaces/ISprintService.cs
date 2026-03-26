using TaskManagementApi.Application.DTOs.Sprints;
using TaskManagementApi.Application.DTOs.Tickets;

namespace TaskManagementApi.Application.Interfaces
{
    public interface ISprintService
    {
        Task<SprintDto> CreateAsync(int projectId, CreateSprintRequest request, int userId);
        Task<IEnumerable<SprintDto>> GetProjectSprintsAsync(int projectId);
        Task<SprintDto?> GetActiveSprintAsync(int projectId, int? subProjectId = null);
        Task<SprintDto?> GetByIdAsync(int id);
        Task UpdateAsync(int id, UpdateSprintRequest request);
        Task DeleteAsync(int id);
        Task ActivateAsync(int id);
        Task CloseAsync(int id, CloseSprintRequest request, int userId);
        Task AddTicketToSprintAsync(int id, int ticketId, string? reason, int userId);
        Task RemoveTicketFromSprintAsync(int id, int ticketId, string? reason, int userId);
        Task<IEnumerable<TicketResponse>> GetSprintTicketsAsync(int id);
        Task<IEnumerable<SprintMemberCapacityDto>> GetMemberCapacitiesAsync(int id);
        Task SetMemberCapacitiesAsync(int id, SetSprintMemberCapacityRequest request);
        Task<SprintSummaryDto> GetSummaryAsync(int id);
        Task<IEnumerable<SprintSummaryDto>> GetHistoryAsync(int projectId);
        Task<IEnumerable<SprintScopeChangeDto>> GetScopeChangesAsync(int id);
        Task<VelocityDto> GetVelocityAsync(int projectId);
    }
}
