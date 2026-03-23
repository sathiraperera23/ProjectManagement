using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Application.DTOs.Projects
{
    public class CreateProjectRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? ClientName { get; set; }
        public string? ProjectCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public ProjectStatus Status { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Colour { get; set; }
    }

    public class UpdateProjectRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? ClientName { get; set; }
        public string? ProjectCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public ProjectStatus Status { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Colour { get; set; }
    }

    public class ProjectResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? ClientName { get; set; }
        public string ProjectCode { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public ProjectStatus Status { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Colour { get; set; }
        public bool IsArchived { get; set; }
    }
}
