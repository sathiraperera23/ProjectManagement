using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Application.DTOs.SubProjects
{
    public class CreateSubProjectRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public SubProjectStatus Status { get; set; }
        public int? ModuleOwnerUserId { get; set; }
        public int? DependsOnSubProjectId { get; set; }
    }

    public class UpdateSubProjectRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public SubProjectStatus Status { get; set; }
        public int? ModuleOwnerUserId { get; set; }
        public int? DependsOnSubProjectId { get; set; }
    }

    public class SubProjectResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public SubProjectStatus Status { get; set; }
        public int? ModuleOwnerUserId { get; set; }
        public int? DependsOnSubProjectId { get; set; }
    }

    public class SubProjectProgressResponse
    {
        public double ProgressPercentage { get; set; }
    }
}
