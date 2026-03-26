using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Application.DTOs.Sprints;
using TaskManagementApi.Application.DTOs.Tickets;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Application.Services
{
    public class SprintService : ISprintService
    {
        private readonly IRepository<Sprint> _sprintRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<SprintMemberCapacity> _capacityRepository;
        private readonly IRepository<SprintScopeChange> _scopeChangeRepository;
        private readonly IRepository<User> _userRepository;

        public SprintService(
            IRepository<Sprint> sprintRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<SprintMemberCapacity> capacityRepository,
            IRepository<SprintScopeChange> scopeChangeRepository,
            IRepository<User> userRepository)
        {
            _sprintRepository = sprintRepository;
            _ticketRepository = ticketRepository;
            _capacityRepository = capacityRepository;
            _scopeChangeRepository = scopeChangeRepository;
            _userRepository = userRepository;
        }

        public async Task<SprintDto> CreateAsync(int projectId, CreateSprintRequest request, int userId)
        {
            var sprint = new Sprint
            {
                Name = request.Name,
                Goal = request.Goal,
                ProjectId = projectId,
                ProductId = request.ProductId,
                SubProjectId = request.SubProjectId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                StoryPointCapacity = request.StoryPointCapacity,
                Status = SprintStatus.Planning,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _sprintRepository.AddAsync(sprint);
            return (await GetByIdAsync(sprint.Id))!;
        }

        public async Task<IEnumerable<SprintDto>> GetProjectSprintsAsync(int projectId)
        {
            var sprints = await _sprintRepository.Query()
                .Include(s => s.Tickets)
                .Where(s => s.ProjectId == projectId)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();

            return sprints.Select(MapToDto);
        }

        public async Task<SprintDto?> GetActiveSprintAsync(int projectId, int? subProjectId = null)
        {
            var query = _sprintRepository.Query().Include(s => s.Tickets).Where(s => s.ProjectId == projectId && s.Status == SprintStatus.Active);
            if (subProjectId.HasValue) query = query.Where(s => s.SubProjectId == subProjectId);

            var sprint = await query.FirstOrDefaultAsync();
            return sprint != null ? MapToDto(sprint) : null;
        }

        public async Task<SprintDto?> GetByIdAsync(int id)
        {
            var sprint = await _sprintRepository.Query().Include(s => s.Tickets).FirstOrDefaultAsync(s => s.Id == id);
            return sprint != null ? MapToDto(sprint) : null;
        }

        public async Task UpdateAsync(int id, UpdateSprintRequest request)
        {
            var sprint = await _sprintRepository.GetByIdAsync(id);
            if (sprint == null) throw new KeyNotFoundException("Sprint not found");

            sprint.Name = request.Name;
            sprint.Goal = request.Goal;
            sprint.StartDate = request.StartDate;
            sprint.EndDate = request.EndDate;
            sprint.StoryPointCapacity = request.StoryPointCapacity;
            sprint.UpdatedAt = DateTime.UtcNow;

            await _sprintRepository.UpdateAsync(sprint);
        }

        public async Task DeleteAsync(int id)
        {
            var sprint = await _sprintRepository.GetByIdAsync(id);
            if (sprint == null) throw new KeyNotFoundException("Sprint not found");
            if (sprint.Status != SprintStatus.Planning) throw new InvalidOperationException("Only Planning sprints can be deleted");

            await _sprintRepository.DeleteAsync(id);
        }

        public async Task ActivateAsync(int id)
        {
            var sprint = await _sprintRepository.GetByIdAsync(id);
            if (sprint == null) throw new KeyNotFoundException("Sprint not found");

            // Only one Active sprint allowed per sub-project at a time
            var anyActive = await _sprintRepository.Query()
                .AnyAsync(s => s.ProjectId == sprint.ProjectId && s.SubProjectId == sprint.SubProjectId && s.Status == SprintStatus.Active);

            if (anyActive) throw new InvalidOperationException("Another sprint is already active for this (sub-)project");

            sprint.Status = SprintStatus.Active;
            sprint.UpdatedAt = DateTime.UtcNow;
            await _sprintRepository.UpdateAsync(sprint);
        }

        public async Task CloseAsync(int id, CloseSprintRequest request, int userId)
        {
            var sprint = await _sprintRepository.Query().Include(s => s.Tickets).ThenInclude(t => t.Status).FirstOrDefaultAsync(s => s.Id == id);
            if (sprint == null) throw new KeyNotFoundException("Sprint not found");
            if (sprint.Status != SprintStatus.Active) throw new InvalidOperationException("Only Active sprints can be closed");

            var incompleteTickets = sprint.Tickets.Where(t => t.Status == null || !t.Status.IsTerminal).ToList();

            foreach (var ticket in incompleteTickets)
            {
                if (request.Disposition == SprintClosureDisposition.MoveToBacklog)
                {
                    ticket.SprintId = null;
                }
                else if (request.Disposition == SprintClosureDisposition.MoveToNextSprint)
                {
                    if (!request.NextSprintId.HasValue) throw new InvalidOperationException("NextSprintId is required for MoveToNextSprint disposition");
                    ticket.SprintId = request.NextSprintId.Value;

                    await _scopeChangeRepository.AddAsync(new SprintScopeChange
                    {
                        SprintId = request.NextSprintId.Value,
                        TicketId = ticket.Id,
                        ChangeType = SprintScopeChangeType.Added,
                        ChangedByUserId = userId,
                        Reason = "Carried over from sprint " + sprint.Name,
                        ChangedAt = DateTime.UtcNow
                    });
                }
                // LeaveInPlace does nothing to the ticket's SprintId

                await _ticketRepository.UpdateAsync(ticket);

                // Log removal from current sprint
                await _scopeChangeRepository.AddAsync(new SprintScopeChange
                {
                    SprintId = sprint.Id,
                    TicketId = ticket.Id,
                    ChangeType = SprintScopeChangeType.Removed,
                    ChangedByUserId = userId,
                    Reason = "Sprint closed, disposition: " + request.Disposition,
                    ChangedAt = DateTime.UtcNow
                });
            }

            sprint.Status = SprintStatus.Closed;
            sprint.ClosedAt = DateTime.UtcNow;
            sprint.UpdatedAt = DateTime.UtcNow;
            await _sprintRepository.UpdateAsync(sprint);
        }

        public async Task AddTicketToSprintAsync(int id, int ticketId, string? reason, int userId)
        {
            var sprint = await _sprintRepository.GetByIdAsync(id);
            if (sprint == null) throw new KeyNotFoundException("Sprint not found");
            if (sprint.Status == SprintStatus.Closed) throw new InvalidOperationException("Cannot add tickets to a closed sprint");

            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null) throw new KeyNotFoundException("Ticket not found");

            ticket.SprintId = id;
            await _ticketRepository.UpdateAsync(ticket);

            if (sprint.Status == SprintStatus.Active)
            {
                await _scopeChangeRepository.AddAsync(new SprintScopeChange
                {
                    SprintId = id,
                    TicketId = ticketId,
                    ChangeType = SprintScopeChangeType.Added,
                    ChangedByUserId = userId,
                    Reason = reason,
                    ChangedAt = DateTime.UtcNow
                });
            }
        }

        public async Task RemoveTicketFromSprintAsync(int id, int ticketId, string? reason, int userId)
        {
            var sprint = await _sprintRepository.GetByIdAsync(id);
            if (sprint == null) throw new KeyNotFoundException("Sprint not found");
            if (sprint.Status == SprintStatus.Closed) throw new InvalidOperationException("Cannot remove tickets from a closed sprint");

            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket != null && ticket.SprintId == id)
            {
                ticket.SprintId = null;
                await _ticketRepository.UpdateAsync(ticket);

                if (sprint.Status == SprintStatus.Active)
                {
                    await _scopeChangeRepository.AddAsync(new SprintScopeChange
                    {
                        SprintId = id,
                        TicketId = ticketId,
                        ChangeType = SprintScopeChangeType.Removed,
                        ChangedByUserId = userId,
                        Reason = reason,
                        ChangedAt = DateTime.UtcNow
                    });
                }
            }
        }

        public async Task<IEnumerable<TicketResponse>> GetSprintTicketsAsync(int id)
        {
            var tickets = await _ticketRepository.Query()
                .Include(t => t.Status)
                .Where(t => t.SprintId == id)
                .ToListAsync();

            return tickets.Select(MapToTicketResponse);
        }

        public async Task<IEnumerable<SprintMemberCapacityDto>> GetMemberCapacitiesAsync(int id)
        {
            var capacities = await _capacityRepository.Query()
                .Include(c => c.User)
                .Where(c => c.SprintId == id)
                .ToListAsync();

            return capacities.Select(c => new SprintMemberCapacityDto
            {
                UserId = c.UserId,
                UserDisplayName = c.User.DisplayName,
                AvailableStoryPoints = c.AvailableStoryPoints,
                AvailabilityPercentage = c.AvailabilityPercentage
            });
        }

        public async Task SetMemberCapacitiesAsync(int id, SetSprintMemberCapacityRequest request)
        {
            var existing = await _capacityRepository.Query().Where(c => c.SprintId == id).ToListAsync();
            foreach (var e in existing) await _capacityRepository.DeleteAsync(e.Id);

            foreach (var c in request.Capacities)
            {
                await _capacityRepository.AddAsync(new SprintMemberCapacity
                {
                    SprintId = id,
                    UserId = c.UserId,
                    AvailableStoryPoints = c.AvailableStoryPoints,
                    AvailabilityPercentage = c.AvailabilityPercentage
                });
            }
        }

        public async Task<SprintSummaryDto> GetSummaryAsync(int id)
        {
            var sprint = await _sprintRepository.Query()
                .Include(s => s.Tickets).ThenInclude(t => t.Status)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sprint == null) throw new KeyNotFoundException("Sprint not found");

            return CalculateSummary(sprint);
        }

        public async Task<IEnumerable<SprintSummaryDto>> GetHistoryAsync(int projectId)
        {
            var sprints = await _sprintRepository.Query()
                .Include(s => s.Tickets).ThenInclude(t => t.Status)
                .Where(s => s.ProjectId == projectId && s.Status == SprintStatus.Closed)
                .OrderByDescending(s => s.ClosedAt)
                .ToListAsync();

            return sprints.Select(CalculateSummary);
        }

        public async Task<IEnumerable<SprintScopeChangeDto>> GetScopeChangesAsync(int id)
        {
            var changes = await _scopeChangeRepository.Query()
                .Include(c => c.Ticket)
                .Include(c => c.ChangedBy)
                .Where(c => c.SprintId == id)
                .OrderByDescending(c => c.ChangedAt)
                .ToListAsync();

            return changes.Select(c => new SprintScopeChangeDto
            {
                TicketId = c.TicketId,
                TicketNumber = c.Ticket.TicketNumber,
                TicketTitle = c.Ticket.Title,
                ChangeType = c.ChangeType,
                ChangedByUserName = c.ChangedBy.DisplayName,
                Reason = c.Reason,
                ChangedAt = c.ChangedAt
            });
        }

        public async Task<VelocityDto> GetVelocityAsync(int projectId)
        {
            var last10 = await _sprintRepository.Query()
                .Include(s => s.Tickets).ThenInclude(t => t.Status)
                .Where(s => s.ProjectId == projectId && s.Status == SprintStatus.Closed)
                .OrderByDescending(s => s.ClosedAt)
                .Take(10)
                .ToListAsync();

            var summaries = last10.Select(CalculateSummary).ToList();
            var rollingAverage = summaries.Any() ? summaries.Average(s => s.CompletedStoryPoints) : 0;

            return new VelocityDto
            {
                SprintHistory = summaries,
                RollingAverageVelocity = rollingAverage
            };
        }

        private SprintSummaryDto CalculateSummary(Sprint sprint)
        {
            var plannedPoints = sprint.Tickets.Sum(t => t.StoryPoints ?? 0);
            var completedPoints = sprint.Tickets
                .Where(t => t.Status != null && t.Status.IsTerminal)
                .Sum(t => t.StoryPoints ?? 0);

            var carriedOver = sprint.Tickets.Count(t => t.Status == null || !t.Status.IsTerminal);

            return new SprintSummaryDto
            {
                SprintId = sprint.Id,
                SprintName = sprint.Name,
                PlannedStoryPoints = plannedPoints,
                CompletedStoryPoints = completedPoints,
                CompletionRate = plannedPoints > 0 ? (double)completedPoints / plannedPoints * 100 : 0,
                CarriedOverTicketCount = carriedOver,
                ClosedAt = sprint.ClosedAt
            };
        }

        private SprintDto MapToDto(Sprint s)
        {
            var totalPlanned = s.Tickets.Sum(t => t.StoryPoints ?? 0);
            return new SprintDto
            {
                Id = s.Id,
                Name = s.Name,
                Goal = s.Goal,
                ProjectId = s.ProjectId,
                ProductId = s.ProductId,
                SubProjectId = s.SubProjectId,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                StoryPointCapacity = s.StoryPointCapacity,
                Status = s.Status,
                CreatedAt = s.CreatedAt,
                TotalPlannedPoints = totalPlanned,
                HasCapacityWarning = totalPlanned > s.StoryPointCapacity
            };
        }

        private TicketResponse MapToTicketResponse(Ticket t)
        {
            return new TicketResponse
            {
                Id = t.Id,
                TicketNumber = t.TicketNumber,
                Title = t.Title,
                StatusName = t.Status?.Name ?? "Unknown",
                StoryPoints = t.StoryPoints,
                Priority = t.Priority,
                ExpectedDueDate = t.ExpectedDueDate
            };
        }
    }
}
