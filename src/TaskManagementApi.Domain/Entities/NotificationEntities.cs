using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;

        public NotificationEventType EventType { get; set; }
        public int? ReferenceId { get; set; }
        public string? ReferenceType { get; set; } // 'Ticket', 'Sprint', 'Milestone'

        public bool IsRead { get; set; }
    }

    public class NotificationPreference : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int? ProjectId { get; set; }
        public Project? Project { get; set; }

        public NotificationEventType EventType { get; set; }

        public bool InApp { get; set; }
        public bool Email { get; set; }
        public bool Sms { get; set; }
        public bool DigestMode { get; set; }
    }

    public class NotificationLog : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string Channel { get; set; } = null!; // InApp, Email, Sms
        public NotificationEventType EventType { get; set; }
        public int? ReferenceId { get; set; }

        public DateTime SentAt { get; set; }
        public DeliveryStatus DeliveryStatus { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class NotificationRule : BaseEntity
    {
        public int? ProjectId { get; set; }
        public Project? Project { get; set; }

        public NotificationEventType EventType { get; set; }
        public RecipientType RecipientType { get; set; }
        public NotificationChannel Channel { get; set; }
        public NotificationTiming ImmediateOrDigest { get; set; }
    }
}
