using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Application.DTOs.SubProjects;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Application.Services
{
    public class SubProjectService : ISubProjectService
    {
        private readonly IRepository<SubProject> _subProjectRepository;
        private readonly IRepository<Ticket> _ticketRepository;

        public SubProjectService(IRepository<SubProject> subProjectRepository, IRepository<Ticket> ticketRepository)
        {
            _subProjectRepository = subProjectRepository;
            _ticketRepository = ticketRepository;
        }

        public async Task<SubProjectResponse> CreateSubProjectAsync(int productId, CreateSubProjectRequest request)
        {
            var subProject = new SubProject
            {
                ProductId = productId,
                Name = request.Name,
                Description = request.Description,
                Status = request.Status,
                ModuleOwnerUserId = request.ModuleOwnerUserId,
                DependsOnSubProjectId = request.DependsOnSubProjectId
            };

            await _subProjectRepository.AddAsync(subProject);

            return MapToResponse(subProject);
        }

        public async Task<IEnumerable<SubProjectResponse>> GetSubProjectsByProductIdAsync(int productId)
        {
            var subProjects = await _subProjectRepository.Query().Where(s => s.ProductId == productId).ToListAsync();
            return subProjects.Select(MapToResponse);
        }

        public async Task<SubProjectResponse?> GetSubProjectByIdAsync(int id)
        {
            var subProject = await _subProjectRepository.GetByIdAsync(id);
            return subProject != null ? MapToResponse(subProject) : null;
        }

        public async Task UpdateSubProjectAsync(int id, UpdateSubProjectRequest request)
        {
            var subProject = await _subProjectRepository.GetByIdAsync(id);
            if (subProject == null) return;

            subProject.Name = request.Name;
            subProject.Description = request.Description;
            subProject.Status = request.Status;
            subProject.ModuleOwnerUserId = request.ModuleOwnerUserId;
            subProject.DependsOnSubProjectId = request.DependsOnSubProjectId;

            await _subProjectRepository.UpdateAsync(subProject);
        }

        public async Task SoftDeleteSubProjectAsync(int id)
        {
            await _subProjectRepository.DeleteAsync(id);
        }

        public async Task AssignTeamToSubProjectAsync(int id, int teamId)
        {
            // Placeholder for team assignment to sub-project
            await Task.CompletedTask;
        }

        public async Task<SubProjectProgressResponse> GetSubProjectProgressAsync(int id)
        {
            var tickets = await _ticketRepository.Query()
                .Include(t => t.Status)
                .Where(t => t.SubProjectId == id)
                .ToListAsync();

            if (!tickets.Any()) return new SubProjectProgressResponse { ProgressPercentage = 0 };

            var completedTickets = tickets.Count(t => t.Status != null && t.Status.IsTerminal);
            var percentage = (double)completedTickets / tickets.Count * 100;

            return new SubProjectProgressResponse { ProgressPercentage = percentage };
        }

        private SubProjectResponse MapToResponse(SubProject subProject)
        {
            return new SubProjectResponse
            {
                Id = subProject.Id,
                ProductId = subProject.ProductId,
                Name = subProject.Name,
                Description = subProject.Description,
                Status = subProject.Status,
                ModuleOwnerUserId = subProject.ModuleOwnerUserId,
                DependsOnSubProjectId = subProject.DependsOnSubProjectId
            };
        }
    }
}
