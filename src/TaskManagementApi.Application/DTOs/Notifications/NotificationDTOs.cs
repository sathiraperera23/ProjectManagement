using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Application.DTOs.Notifications
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public NotificationEventType EventType { get; set; }
        public int? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class NotificationPreferenceDto
    {
        public int? ProjectId { get; set; }
        public NotificationEventType EventType { get; set; }
        public bool InApp { get; set; }
        public bool Email { get; set; }
        public bool Sms { get; set; }
        public bool DigestMode { get; set; }
    }

    public class UpdateNotificationPreferenceRequest
    {
        public List<NotificationPreferenceDto> Preferences { get; set; } = new();
    }

    public class NotificationRuleDto
    {
        public int Id { get; set; }
        public int? ProjectId { get; set; }
        public NotificationEventType EventType { get; set; }
        public RecipientType RecipientType { get; set; }
        public NotificationChannel Channel { get; set; }
        public NotificationTiming ImmediateOrDigest { get; set; }
    }

    public class NotificationLogDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserDisplayName { get; set; } = null!;
        public string Channel { get; set; } = null!;
        public NotificationEventType EventType { get; set; }
        public DateTime SentAt { get; set; }
        public string DeliveryStatus { get; set; } = null!;
        public string? ErrorMessage { get; set; }
    }
}
