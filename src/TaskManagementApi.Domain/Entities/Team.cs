namespace TaskManagementApi.Domain.Entities
{
    public class Team : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
    }
}
