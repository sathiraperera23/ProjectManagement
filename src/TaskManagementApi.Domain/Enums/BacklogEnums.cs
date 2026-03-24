namespace TaskManagementApi.Domain.Enums
{
    public enum BacklogItemType { BRD, UserStory, UseCase, Epic, ChangeRequest }
    public enum BacklogItemStatus { Draft, Approved, InProgress, Done }
    public enum BacklogItemPriority { Critical, High, Medium, Low }
    public enum ApprovalRequestStatus { Pending, Approved, Rejected, ChangesRequested }
}
