using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Domain.Entities
{
    public class Product : BaseEntity
    {
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public string VersionName { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime PlannedReleaseDate { get; set; }
        public ReleaseType ReleaseType { get; set; }
        public ProductStatus Status { get; set; }

        public ICollection<SubProject> SubProjects { get; set; } = new List<SubProject>();
    }
}
