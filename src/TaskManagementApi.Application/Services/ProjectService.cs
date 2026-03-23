using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Application.DTOs.Projects;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Entities;

namespace TaskManagementApi.Application.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IRepository<Project> _projectRepository;

        public ProjectService(IRepository<Project> projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request)
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
                StartDate = request.StartDate,
                ExpectedEndDate = request.ExpectedEndDate,
                Status = request.Status,
                AvatarUrl = request.AvatarUrl,
                Colour = request.Colour
            };

            await _projectRepository.AddAsync(project);

            return MapToResponse(project);
        }

        public async Task<IEnumerable<ProjectResponse>> GetAllProjectsAsync()
        {
            var projects = await _projectRepository.GetAllAsync();
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
            project.StartDate = request.StartDate;
            project.ExpectedEndDate = request.ExpectedEndDate;
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
            // Placeholder: Team and User assignment logic (e.g., adding to a join table)
            await Task.CompletedTask;
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
