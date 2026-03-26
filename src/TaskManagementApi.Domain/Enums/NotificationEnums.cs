namespace TaskManagementApi.Domain.Enums
{
    public enum NotificationEventType
    {
        TicketCreated,
        TicketAssigned,
        TicketStatusChanged,
        TicketCommentedOn,
        Mention,
        DueIn7Days,
        DueIn3Days,
        DueIn1Day,
        DueToday,
        TicketOverdue,
        TicketOverdue3Days,
        TicketPaused,
        TicketBlocked,
        TicketReopened,
        ClientBugReceived,
        BugApproved,
        BugRejected,
        SprintStarted,
        SprintOverrunRisk,
        SprintClosed,
        MilestoneAtRisk,
        MilestoneOverdue,
        Milestone7DayAlert,
        DailyUpdateBlocker,
        WipLimitExceeded,
        AccessRequest
    }

    public enum NotificationChannel
    {
        InApp = 1,
        Email = 2,
        Sms = 4,
        All = 7
    }

    public enum RecipientType
    {
        Assignee,
        Reporter,
        Watcher,
        TeamLead,
        ProjectManager,
        Director,
        SubProjectOwner,
        SpecificUser,
        SpecificRole,
        SpecificTeam,
        ExternalEmail
    }

    public enum DeliveryStatus
    {
        Sent,
        Delivered,
        Bounced,
        Opened,
        Failed
    }

    public enum NotificationTiming
    {
        Immediate,
        Digest
    }
}
