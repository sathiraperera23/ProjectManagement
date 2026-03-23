namespace TaskManagementApi.Domain.Constants
{
    public static class Permissions
    {
        // Project
        public const string CreateProject     = "CREATE_PROJECT";
        public const string EditProject       = "EDIT_PROJECT";
        public const string ArchiveProject    = "ARCHIVE_PROJECT";
        public const string DeleteProject     = "DELETE_PROJECT";
        public const string ViewAllProjects   = "VIEW_ALL_PROJECTS";

        // Product
        public const string CreateProduct        = "CREATE_PRODUCT";
        public const string EditProduct          = "EDIT_PRODUCT";
        public const string ManageReleaseNotes   = "MANAGE_RELEASE_NOTES";
        public const string ViewProductBacklog   = "VIEW_PRODUCT_BACKLOG";

        // Ticket
        public const string CreateTicket      = "CREATE_TICKET";
        public const string EditOwnTickets    = "EDIT_OWN_TICKETS";
        public const string EditAllTickets    = "EDIT_ALL_TICKETS";
        public const string DeleteTicket      = "DELETE_TICKET";
        public const string ReassignTicket    = "REASSIGN_TICKET";
        public const string ChangeStatus      = "CHANGE_STATUS";
        public const string ViewAllTickets    = "VIEW_ALL_TICKETS";
        public const string ApproveTickets    = "APPROVE_TICKETS";

        // Backlog
        public const string ManageBrds             = "MANAGE_BRDS";
        public const string ManageUserStories      = "MANAGE_USER_STORIES";
        public const string ApproveRequirements    = "APPROVE_REQUIREMENTS";

        // Sprint
        public const string CreateSprint         = "CREATE_SPRINT";
        public const string CloseSprint          = "CLOSE_SPRINT";
        public const string MoveTicketsToSprint  = "MOVE_TICKETS_TO_SPRINT";

        // Report
        public const string ViewReports      = "VIEW_REPORTS";
        public const string ExportReports    = "EXPORT_REPORTS";
        public const string ViewCostingData  = "VIEW_COSTING_DATA";
        public const string ViewBudgetData   = "VIEW_BUDGET_DATA";

        // Settings
        public const string ManageUsers                = "MANAGE_USERS";
        public const string ManageRoles                = "MANAGE_ROLES";
        public const string ManageNotificationSettings = "MANAGE_NOTIFICATION_SETTINGS";
        public const string ManageAccessRules          = "MANAGE_ACCESS_RULES";
    }
}
