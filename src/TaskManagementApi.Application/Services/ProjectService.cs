using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Application.DTOs.Projects;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Entities;

namespace TaskManagementApi.Application.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<TicketStatus> _statusRepository;
        private readonly IUserAdminService _userAdminService;
        private readonly IRepository<Team> _teamRepository;

        public ProjectService(
            IRepository<Project> projectRepository,
            IRepository<TicketStatus> statusRepository,
            IUserAdminService userAdminService,
            IRepository<Team> teamRepository)
        {
            _projectRepository = projectRepository;
            _statusRepository = statusRepository;
            _userAdminService = userAdminService;
            _teamRepository = teamRepository;
        }

        public async Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, int creatorId)
        {
            var projectCode = request.ProjectCode;
            if (string.IsNullOrWhiteSpace(projectCode))
            {
                projectCode = request.Name.Length >= 3
                    ? request.Name.Substring(0, 3).ToUpper()
                    : request.Name.ToUpper().PadRight(3, 'X');
            }
            else
            {
                projectCode = projectCode.ToUpper();
            }

            var project = new Project
            {
                Name = request.Name,
                Description = request.Description,
                ClientName = request.ClientName,
                ProjectCode = projectCode,
                StartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc),
                ExpectedEndDate = request.ExpectedEndDate.HasValue ? DateTime.SpecifyKind(request.ExpectedEndDate.Value, DateTimeKind.Utc) : null,
                Status = request.Status,
                AvatarUrl = request.AvatarUrl,
                Colour = request.Colour
            };

            await _projectRepository.AddAsync(project);

            // Seed default statuses for the new project using repo
            await SeedTicketStatusesAsync(project.Id);

            // Seed default teams
            await _userAdminService.SeedDefaultTeamsAsync(project.Id);

            // Assign creator as PM
            var pmTeam = await _teamRepository.Query()
                .FirstOrDefaultAsync(t => t.ProjectId == project.Id && t.Name == "Project Management");
            if (pmTeam != null)
            {
                await _userAdminService.AddMemberToTeamAsync(pmTeam.Id, creatorId);
            }

            return MapToResponse(project);
        }

        private async Task SeedTicketStatusesAsync(int projectId)
        {
            var statuses = new List<TicketStatus>
            {
                new() { ProjectId = projectId, Name = "Open", Colour = "#CCCCCC", Order = 1, IsDefault = true, IsTerminal = false },
                new() { ProjectId = projectId, Name = "Completed", Colour = "#33CC33", Order = 9, IsDefault = false, IsTerminal = true },
                new() { ProjectId = projectId, Name = "Closed", Colour = "#666666", Order = 10, IsDefault = false, IsTerminal = true },
                new() { ProjectId = projectId, Name = "Cancelled", Colour = "#FF3300", Order = 11, IsDefault = false, IsTerminal = true }
            };

            foreach (var status in statuses)
            {
                await _statusRepository.AddAsync(status);
            }
        }

        public async Task<IEnumerable<ProjectResponse>> GetAllProjectsAsync(int? userId = null)
        {
            IQueryable<Project> query = _projectRepository.Query();

            if (userId.HasValue)
            {
                var assignedProjectIds = await _teamRepository.Query()
                    .Where(t => t.Members.Any(m => m.UserId == userId.Value))
                    .Select(t => t.ProjectId)
                    .Distinct()
                    .ToListAsync();

                query = query.Where(p => assignedProjectIds.Contains(p.Id));
            }

            var projects = await query.ToListAsync();
            return projects.Select(MapToResponse);
        }

        public async Task<ProjectResponse?> GetProjectByIdAsync(int id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            return project != null ? MapToResponse(project) : null;
        }

        public async Task UpdateProjectAsync(int id, UpdateProjectRequest request)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null || project.IsArchived) return;

            project.Name = request.Name;
            project.Description = request.Description;
            project.ClientName = request.ClientName;
            if (!string.IsNullOrWhiteSpace(request.ProjectCode))
            {
                project.ProjectCode = request.ProjectCode.ToUpper();
            }
            project.StartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc);
            project.ExpectedEndDate = request.ExpectedEndDate.HasValue ? DateTime.SpecifyKind(request.ExpectedEndDate.Value, DateTimeKind.Utc) : null;
            project.Status = request.Status;
            project.AvatarUrl = request.AvatarUrl;
            project.Colour = request.Colour;

            await _projectRepository.UpdateAsync(project);
        }

        public async Task ArchiveProjectAsync(int id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project != null)
            {
                project.IsArchived = true;
                await _projectRepository.UpdateAsync(project);
            }
        }

        public async Task SoftDeleteProjectAsync(int id)
        {
            await _projectRepository.DeleteAsync(id);
        }

        public async Task AssignTeamOrUserAsync(int id, int? teamId, int? userId)
        {
            await Task.CompletedTask;
        }

        public async Task<bool> IsUserAssignedToProjectAsync(int userId, int projectId)
        {
            return await _teamRepository.Query()
                .Where(t => t.ProjectId == projectId)
                .AnyAsync(t => t.Members.Any(m => m.UserId == userId));
        }

        private ProjectResponse MapToResponse(Project project)
        {
            return new ProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                ClientName = project.ClientName,
                ProjectCode = project.ProjectCode,
                StartDate = project.StartDate,
                ExpectedEndDate = project.ExpectedEndDate,
                Status = project.Status,
                AvatarUrl = project.AvatarUrl,
                Colour = project.Colour,
                IsArchived = project.IsArchived
            };
        }
    }
}
