using TaskManagementApi.Application.DTOs.Tickets;
using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Application.Interfaces
{
    public interface ITicketService
    {
        Task<TicketResponse> CreateTicketAsync(CreateTicketRequest request, int reporterId);
        Task<IEnumerable<TicketResponse>> GetAllTicketsAsync(
            int? projectId = null,
            int? productId = null,
            int? subProjectId = null,
            int? statusId = null,
            TicketPriority? priority = null,
            TicketCategory? category = null,
            int? assigneeId = null,
            int? teamId = null,
            int? sprintId = null,
            int? milestoneId = null,
            string? label = null,
            DateTime? dueDateFrom = null,
            DateTime? dueDateTo = null);
        Task<TicketResponse?> GetTicketByIdAsync(int id);
        Task UpdateTicketAsync(int id, UpdateTicketRequest request, int userId);
        Task UpdateTicketStatusAsync(int id, UpdateTicketStatusRequest request, int userId);
        Task AssignTicketAsync(int id, List<int> assigneeIds, int userId);
        Task SoftDeleteTicketAsync(int id);

        // Bulk operations
        Task BulkUpdateStatusAsync(BulkUpdateStatusRequest request, int userId);
        Task BulkAssignAsync(BulkAssignRequest request, int userId);
        Task BulkUpdatePriorityAsync(BulkUpdatePriorityRequest request, int userId);

        // Links
        Task LinkTicketsAsync(int id, LinkTicketRequest request);
        Task RemoveLinkAsync(int id, int linkId);

        // History
        Task<IEnumerable<TicketStatusHistoryResponse>> GetTicketHistoryAsync(int id);

        // Status Management
        Task<IEnumerable<TicketStatusResponse>> GetStatusesByProjectIdAsync(int projectId);
        Task<TicketStatusResponse> CreateStatusAsync(int projectId, CreateTicketStatusRequest request);
        Task UpdateStatusAsync(int id, CreateTicketStatusRequest request);
        Task DeleteStatusAsync(int id);
    }
}
