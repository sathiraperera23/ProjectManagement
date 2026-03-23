namespace TaskManagementApi.Domain.Entities
{
    public class Ticket : BaseEntity
    {
        public int SubProjectId { get; set; }
        public SubProject SubProject { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
    }
}
