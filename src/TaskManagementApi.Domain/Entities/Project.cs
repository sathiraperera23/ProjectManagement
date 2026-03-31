using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Domain.Entities
{
    public class Project : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? ClientName { get; set; }
        public string ProjectCode { get; set; } = null!; // Unique short code (e.g., 'UMS')
        public DateTime StartDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public ProjectStatus Status { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Colour { get; set; }
        public bool IsArchived { get; set; }
        public string? IntakeEmailAddress { get; set; }
        public string? BugReportTemplateCustomText { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<UserProjectRole> UserProjectRoles { get; set; } = new List<UserProjectRole>();
    }
}
