using Microsoft.AspNetCore.Http;
using TaskManagementApi.Application.DTOs.Tickets;

namespace TaskManagementApi.Application.Interfaces
{
    public interface ITicketExtraService
    {
        // Comments
        Task<IEnumerable<CommentDto>> GetTicketCommentsAsync(int ticketId, bool includeInternal);
        Task<CommentDto> PostCommentAsync(int ticketId, CreateCommentRequest request, int userId);
        Task UpdateCommentAsync(int commentId, UpdateCommentRequest request, int userId);
        Task DeleteCommentAsync(int commentId, int userId);
        Task AddReactionAsync(int commentId, string emoji, int userId);
        Task RemoveReactionAsync(int commentId, string emoji, int userId);

        // Watchers
        Task WatchTicketAsync(int ticketId, int userId);
        Task UnwatchTicketAsync(int ticketId, int userId);
        Task<IEnumerable<int>> GetWatchersAsync(int ticketId);

        // Attachments
        Task<TicketAttachmentDto> UploadAttachmentAsync(int ticketId, IFormFile file, int userId);
        Task<TicketAttachmentDto> LinkExternalAttachmentAsync(int ticketId, LinkExternalAttachmentRequest request, int userId);
        Task<IEnumerable<TicketAttachmentDto>> GetTicketAttachmentsAsync(int ticketId);
        Task<TicketAttachmentDto?> GetAttachmentByIdAsync(int attachmentId);
        Task DeleteAttachmentAsync(int ticketId, int attachmentId, int userId);

        // Daily Updates
        Task<DailyUpdateDto> PostDailyUpdateAsync(CreateDailyUpdateRequest request, int userId);
        Task UpdateDailyUpdateAsync(int id, CreateDailyUpdateRequest request, int userId);
        Task<IEnumerable<DailyUpdateDto>> GetUserDailyUpdatesAsync(int userId);
        Task<IEnumerable<DailyUpdateDto>> GetProjectDailyUpdatesAsync(int projectId, int? userId, DateTime? date);
        Task LinkUpdateToTicketAsync(int dailyUpdateId, int ticketId, int userId);
    }
}
