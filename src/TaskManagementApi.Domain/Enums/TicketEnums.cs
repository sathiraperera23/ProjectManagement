namespace TaskManagementApi.Domain.Enums
{
    public enum TicketCategory
    {
        Task,
        Bug,
        Feature,
        Improvement,
        ChangeRequest,
        UserStory,
        TestCase
    }

    public enum TicketPriority
    {
        Critical,
        High,
        Medium,
        Low
    }

    public enum TicketSeverity
    {
        Critical,
        Major,
        Minor,
        Trivial
    }

    public enum TicketEnvironment
    {
        Dev,
        QA,
        UAT,
        Staging,
        Production
    }

    public enum ApprovalStatus
    {
        PendingApproval,
        Approved,
        Rejected,
        AwaitingCustomerReply
    }

    public enum TicketLinkType
    {
        Blocks,
        IsBlockedBy,
        RelatesTo,
        Duplicates
    }
}
