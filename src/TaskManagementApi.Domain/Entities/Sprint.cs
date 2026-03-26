using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Domain.Entities
{
    public class Sprint : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Goal { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        public int? SubProjectId { get; set; }
        public SubProject? SubProject { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int StoryPointCapacity { get; set; }
        public SprintStatus Status { get; set; }

        public int CreatedByUserId { get; set; }
        public User CreatedBy { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<SprintMemberCapacity> MemberCapacities { get; set; } = new List<SprintMemberCapacity>();
        public ICollection<SprintScopeChange> ScopeChanges { get; set; } = new List<SprintScopeChange>();
    }

    public class SprintMemberCapacity : BaseEntity
    {
        public int SprintId { get; set; }
        public Sprint Sprint { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int AvailableStoryPoints { get; set; }
        public double AvailabilityPercentage { get; set; }
    }

    public class SprintScopeChange : BaseEntity
    {
        public int SprintId { get; set; }
        public Sprint Sprint { get; set; } = null!;

        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        public SprintScopeChangeType ChangeType { get; set; }
        public int ChangedByUserId { get; set; }
        public User ChangedBy { get; set; } = null!;

        public string? Reason { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
