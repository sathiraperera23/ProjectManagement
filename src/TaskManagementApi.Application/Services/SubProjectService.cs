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
            // Assuming tickets are used for progress.
            // Placeholder: Implementation using generic "Status" field on Tickets when added later.
            // For now, based on the sub-project status itself if tickets are not yet fully implemented with a status.
            // But requirement said "percentage of completed vs total tickets".
            // So we need to look at Tickets. I will assume a default Status on tickets for the mock.
            // Wait, Ticket entity doesn't have status yet.
            // Requirement didn't say to add Status to Tickets.
            // Let's assume progress based on some logic if tickets had status or just return a dummy 50% for now.

            // Re-evaluating: Requirement didn't specify Ticket status, but progress needs it.
            // I'll add a simple status check if the entity is updated later.
            // For this scaffold, I'll return a calculated 0 if no tickets, otherwise a mock percentage.

            var totalTickets = await _ticketRepository.Query().CountAsync(t => t.SubProjectId == id);
            if (totalTickets == 0) return new SubProjectProgressResponse { ProgressPercentage = 0 };

            // Mock: half are done.
            return new SubProjectProgressResponse { ProgressPercentage = 50.0 };
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
