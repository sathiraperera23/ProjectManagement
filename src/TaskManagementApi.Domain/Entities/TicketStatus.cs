namespace TaskManagementApi.Domain.Entities
{
    public class TicketStatus : BaseEntity
    {
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Colour { get; set; }
        public int Order { get; set; }
        public bool IsDefault { get; set; }
        public bool IsTerminal { get; set; } // Completed/Closed statuses
    }
}
