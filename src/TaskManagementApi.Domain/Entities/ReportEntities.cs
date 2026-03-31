namespace TaskManagementApi.Domain.Entities
{
    public class ProjectBudget : BaseEntity
    {
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public int? SubProjectId { get; set; }
        public SubProject? SubProject { get; set; }

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        public decimal BudgetAmount { get; set; }
        public decimal? ContractValue { get; set; }

        public int SetByUserId { get; set; }
        public User SetByUser { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }

    public class UserRate : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public decimal HourlyRate { get; set; }
        public decimal DailyRate { get; set; }

        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }

    public class TimeLog : BaseEntity
    {
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public decimal HoursLogged { get; set; }
        public DateTime LoggedAt { get; set; }
        public string? Description { get; set; }
    }

    public enum DelayType
    {
        Overdue,
        NotStartedLate,
        PausedExtended,
        SprintOverrun,
        MilestoneAtRisk,
        SubProjectDelayed,
        BlockedUnresolved
    }

    public class DelayRecord : BaseEntity
    {
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        public DelayType DelayType { get; set; }
        public DateTime DetectedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public int DaysDelayed { get; set; }
        public string? Reason { get; set; }
        public DateTime? RevisedDueDate { get; set; }
        public int EscalationLevel { get; set; } // 0=none, 1=PM notified, 2=Director notified
    }

    public class EscalationRule : BaseEntity
    {
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public int EscalateAfterDays { get; set; } = 3;
        public int RepeatEveryDays { get; set; }
        public int SecondLevelAfterDays { get; set; }
    }
}
