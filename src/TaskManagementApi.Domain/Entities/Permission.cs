namespace TaskManagementApi.Domain.Entities
{
    public class Permission : BaseEntity
    {
        public string Name { get; set; } = null!;        // e.g. "CREATE_TICKET"
        public string DisplayName { get; set; } = null!; // e.g. "Create Ticket"
        public string Group { get; set; } = null!;       // e.g. "Ticket", "Project", "Sprint"
        public string Description { get; set; } = null!;
    }
}
