using Microsoft.AspNetCore.Http;
using TaskManagementApi.Application.DTOs.Backlog;
using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Application.Interfaces
{
    public interface IBacklogService
    {
        Task<BacklogItemDto?> GetByIdAsync(int id);
        Task<IEnumerable<BacklogItemDto>> GetProjectBacklogAsync(int projectId, BacklogFilterRequest filter);
        Task<IEnumerable<BacklogItemDto>> GetProductBacklogAsync(int productId, BacklogFilterRequest filter);
        Task<BacklogItemDto> CreateAsync(CreateBacklogItemRequest request, int createdByUserId);
        Task<BacklogItemDto> UpdateAsync(int id, UpdateBacklogItemRequest request, int updatedByUserId);
        Task DeleteAsync(int id, int deletedByUserId);
        Task ReorderAsync(int projectId, ReorderBacklogRequest request);
        Task<IEnumerable<BacklogItemVersionDto>> GetVersionHistoryAsync(int id);
        Task<BacklogItemVersionDto?> GetVersionAsync(int id, int versionNumber);
        Task<BacklogAttachmentDto> AddAttachmentAsync(int id, IFormFile file, int uploadedByUserId);
        Task DeleteAttachmentAsync(int id, int attachmentId);
        Task<BacklogAttachmentDto?> GetAttachmentAsync(int attachmentId);
        Task LinkToTicketAsync(int id, int ticketId, int linkedByUserId);
        Task UnlinkFromTicketAsync(int id, int ticketId);
        Task<IEnumerable<BacklogItemDto>> GetLinkedItemsForTicketAsync(int ticketId);
        Task<BacklogApprovalRequestDto> SubmitForApprovalAsync(int id, int requestedByUserId);
        Task<BacklogApprovalRequestDto> ApproveAsync(int id, int approvalRequestId, string? note, int reviewedByUserId);
        Task<BacklogApprovalRequestDto> RejectAsync(int id, int approvalRequestId, string reason, int reviewedByUserId);
        Task<BacklogApprovalRequestDto> RequestChangesAsync(int id, int approvalRequestId, string note, int reviewedByUserId);
    }
}
