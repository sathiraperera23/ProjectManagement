using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Domain.Entities
{
    public enum AccessComponentType
    {
        Project,
        Product,
        SubProject,
        Report,
        Backlog
    }

    public enum AccessConditionType
    {
        Role,
        Team,
        ProjectAssignment
    }

    public enum AccessLevel
    {
        NoAccess = 0,
        ViewOnly = 1,
        Edit = 2,
        FullAccess = 3
    }

    public enum AccessRequestStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class AccessRule : BaseEntity
    {
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public AccessComponentType ComponentType { get; set; }
        public int ComponentId { get; set; } // Specific ID of Project/Product/etc.

        public AccessConditionType ConditionType { get; set; }
        public string ConditionValue { get; set; } = null!; // Role name, Team name, or User ID

        public AccessLevel AccessLevel { get; set; }
        public bool IsOverride { get; set; } // Explicit allow overrides restrictive rules

        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AccessRequest : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public AccessComponentType ComponentType { get; set; }
        public int ComponentId { get; set; }
        public AccessLevel RequestedAccessLevel { get; set; }
        public AccessRequestStatus Status { get; set; }

        public string? RequestNote { get; set; }
        public int? ReviewedByUserId { get; set; }
        public string? ReviewNote { get; set; }

        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }

    public class ProjectWipLimit : BaseEntity
    {
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public int? SubProjectId { get; set; } // Null if project-level limit
        public int MaxWip { get; set; }
    }

    public class Milestone : BaseEntity
    {
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public string Name { get; set; } = null!;
        public DateTime TargetDate { get; set; }
        public string? Description { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
