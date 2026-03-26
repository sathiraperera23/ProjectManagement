using TaskManagementApi.Application.DTOs.Notifications;
using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendAsync(NotificationEvent e);
        Task MarkAsReadAsync(int notificationId, int userId);
        Task MarkAllAsReadAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId, int page, int pageSize);

        Task<IEnumerable<NotificationPreferenceDto>> GetUserPreferencesAsync(int userId);
        Task UpdateUserPreferencesAsync(int userId, UpdateNotificationPreferenceRequest request);

        Task<IEnumerable<NotificationRuleDto>> GetProjectRulesAsync(int projectId);
        Task UpdateProjectRulesAsync(int projectId, List<NotificationRuleDto> rules);

        Task<IEnumerable<NotificationLogDto>> GetNotificationLogsAsync(int page, int pageSize);
    }

    public class NotificationEvent
    {
        public NotificationEventType Type { get; set; }
        public int? ProjectId { get; set; }
        public int? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public int? SpecificUserId { get; set; }
        public string? ExternalEmail { get; set; }
        public DateTime? RevisedDueDate { get; set; } // For escalation suppression check
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    public interface ISmsService
    {
        Task SendSmsAsync(string to, string message);
    }
}
