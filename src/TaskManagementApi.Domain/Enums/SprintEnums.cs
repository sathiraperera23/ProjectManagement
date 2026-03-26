namespace TaskManagementApi.Domain.Enums
{
    public enum SprintStatus
    {
        Planning,
        Active,
        Closed
    }

    public enum SprintScopeChangeType
    {
        Added,
        Removed
    }

    public enum SprintClosureDisposition
    {
        MoveToBacklog,
        MoveToNextSprint,
        LeaveInPlace
    }
}
