using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Application.DTOs.Sprints
{
    public class SprintDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Goal { get; set; }
        public int ProjectId { get; set; }
        public int? ProductId { get; set; }
        public int? SubProjectId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int StoryPointCapacity { get; set; }
        public SprintStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalPlannedPoints { get; set; }
        public bool HasCapacityWarning { get; set; }
    }

    public class CreateSprintRequest
    {
        public string Name { get; set; } = null!;
        public string? Goal { get; set; }
        public int? ProductId { get; set; }
        public int? SubProjectId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int StoryPointCapacity { get; set; }
    }

    public class UpdateSprintRequest
    {
        public string Name { get; set; } = null!;
        public string? Goal { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int StoryPointCapacity { get; set; }
    }

    public class CloseSprintRequest
    {
        public SprintClosureDisposition Disposition { get; set; }
        public int? NextSprintId { get; set; }
    }

    public class SprintMemberCapacityDto
    {
        public int UserId { get; set; }
        public string UserDisplayName { get; set; } = null!;
        public int AvailableStoryPoints { get; set; }
        public double AvailabilityPercentage { get; set; }
    }

    public class SetSprintMemberCapacityRequest
    {
        public List<SprintMemberCapacityDto> Capacities { get; set; } = new();
    }

    public class SprintSummaryDto
    {
        public int SprintId { get; set; }
        public string SprintName { get; set; } = null!;
        public int PlannedStoryPoints { get; set; }
        public int CompletedStoryPoints { get; set; }
        public double CompletionRate { get; set; }
        public int CarriedOverTicketCount { get; set; }
        public DateTime? ClosedAt { get; set; }
    }

    public class SprintScopeChangeDto
    {
        public int TicketId { get; set; }
        public string TicketNumber { get; set; } = null!;
        public string TicketTitle { get; set; } = null!;
        public SprintScopeChangeType ChangeType { get; set; }
        public string ChangedByUserName { get; set; } = null!;
        public string? Reason { get; set; }
        public DateTime ChangedAt { get; set; }
    }

    public class VelocityDto
    {
        public List<SprintSummaryDto> SprintHistory { get; set; } = new();
        public double RollingAverageVelocity { get; set; }
    }
}
