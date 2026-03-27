using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Application.DTOs.Tickets;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Application.Services
{
    public class TicketService : ITicketService
    {
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<TicketStatus> _statusRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<TicketStatusHistory> _historyRepository;
        private readonly IRepository<TicketLink> _linkRepository;

        public TicketService(
            IRepository<Ticket> ticketRepository,
            IRepository<TicketStatus> statusRepository,
            IRepository<Project> projectRepository,
            IRepository<TicketStatusHistory> historyRepository,
            IRepository<TicketLink> linkRepository)
        {
            _ticketRepository = ticketRepository;
            _statusRepository = statusRepository;
            _projectRepository = projectRepository;
            _historyRepository = historyRepository;
            _linkRepository = linkRepository;
        }

        public async Task<TicketResponse> CreateTicketAsync(CreateTicketRequest request, int reporterId)
        {
            var project = await _projectRepository.GetByIdAsync(request.ProjectId);
            if (project == null) throw new Exception("Project not found");

            var defaultStatus = await _statusRepository.Query()
                .FirstOrDefaultAsync(s => s.ProjectId == request.ProjectId && s.IsDefault)
                ?? await _statusRepository.Query().FirstOrDefaultAsync(s => s.ProjectId == request.ProjectId);

            if (defaultStatus == null) throw new Exception("No ticket status defined for project");

            // Generate Ticket Number: ProjectCode + sequence
            var ticketCount = await _ticketRepository.Query().CountAsync(t => t.ProjectId == request.ProjectId);
            var ticketNumber = $"{project.ProjectCode}-{ticketCount + 1:D3}";

            var ticket = new Ticket
            {
                TicketNumber = ticketNumber,
                Title = request.Title,
                Description = request.Description,
                Category = request.Category,
                Priority = request.Priority,
                ProjectId = request.ProjectId,
                ProductId = request.ProductId,
                SubProjectId = request.SubProjectId,
                StatusId = defaultStatus.Id,
                TeamId = request.TeamId,
                ReporterId = reporterId,
                StartDate = request.StartDate,
                ExpectedDueDate = request.ExpectedDueDate,
                StoryPoints = request.StoryPoints,
                SprintId = request.SprintId,
                MilestoneId = request.MilestoneId,
                BrdNumber = request.BrdNumber,
                UseCaseNumber = request.UseCaseNumber,
                TestCaseNumber = request.TestCaseNumber,
                Severity = request.Severity,
                StepsToReproduce = request.StepsToReproduce,
                ExpectedBehaviour = request.ExpectedBehaviour,
                ActualBehaviour = request.ActualBehaviour,
                Environment = request.Environment
            };

            // Add Assignees
            foreach (var id in request.AssigneeIds)
            {
                ticket.Assignees.Add(new TicketAssignee { UserId = id });
            }

            // Add Labels
            foreach (var label in request.Labels)
            {
                ticket.Labels.Add(new TicketLabel { Label = label });
            }

            await _ticketRepository.AddAsync(ticket);

            return MapToResponse(ticket);
        }

        public async Task<IEnumerable<TicketResponse>> GetAllTicketsAsync(
            int? projectId = null, int? productId = null, int? subProjectId = null,
            int? statusId = null, TicketPriority? priority = null, TicketCategory? category = null,
            int? assigneeId = null, int? teamId = null, int? sprintId = null,
            int? milestoneId = null, string? label = null, DateTime? dueDateFrom = null, DateTime? dueDateTo = null)
        {
            var query = _ticketRepository.Query()
                .Include(t => t.Status)
                .Include(t => t.Assignees)
                .Include(t => t.Labels)
                .AsQueryable();

            if (projectId.HasValue) query = query.Where(t => t.ProjectId == projectId);
            if (productId.HasValue) query = query.Where(t => t.ProductId == productId);
            if (subProjectId.HasValue) query = query.Where(t => t.SubProjectId == subProjectId);
            if (statusId.HasValue) query = query.Where(t => t.StatusId == statusId);
            if (priority.HasValue) query = query.Where(t => t.Priority == priority);
            if (category.HasValue) query = query.Where(t => t.Category == category);
            if (assigneeId.HasValue) query = query.Where(t => t.Assignees.Any(a => a.UserId == assigneeId));
            if (teamId.HasValue) query = query.Where(t => t.TeamId == teamId);
            if (sprintId.HasValue) query = query.Where(t => t.SprintId == sprintId);
            if (milestoneId.HasValue) query = query.Where(t => t.MilestoneId == milestoneId);
            if (!string.IsNullOrEmpty(label)) query = query.Where(t => t.Labels.Any(l => l.Label == label));
            if (dueDateFrom.HasValue) query = query.Where(t => t.ExpectedDueDate >= dueDateFrom);
            if (dueDateTo.HasValue) query = query.Where(t => t.ExpectedDueDate <= dueDateTo);

            var tickets = await query.ToListAsync();
            return tickets.Select(MapToResponse);
        }

        public async Task<TicketResponse?> GetTicketByIdAsync(int id)
        {
            var ticket = await _ticketRepository.Query()
                .Include(t => t.Status)
                .Include(t => t.Assignees)
                .Include(t => t.Labels)
                .FirstOrDefaultAsync(t => t.Id == id);

            return ticket != null ? MapToResponse(ticket) : null;
        }

        public async Task UpdateTicketAsync(int id, UpdateTicketRequest request, int userId)
        {
            var ticket = await _ticketRepository.Query()
                .Include(t => t.Assignees)
                .Include(t => t.Labels)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return;

            ticket.Title = request.Title;
            ticket.Description = request.Description;
            ticket.Category = request.Category;
            ticket.Priority = request.Priority;
            ticket.TeamId = request.TeamId;
            ticket.StartDate = request.StartDate;
            ticket.ExpectedDueDate = request.ExpectedDueDate;
            ticket.StoryPoints = request.StoryPoints;
            ticket.SprintId = request.SprintId;
            ticket.MilestoneId = request.MilestoneId;
            ticket.BrdNumber = request.BrdNumber;
            ticket.UseCaseNumber = request.UseCaseNumber;
            ticket.TestCaseNumber = request.TestCaseNumber;
            ticket.Severity = request.Severity;
            ticket.StepsToReproduce = request.StepsToReproduce;
            ticket.ExpectedBehaviour = request.ExpectedBehaviour;
            ticket.ActualBehaviour = request.ActualBehaviour;
            ticket.Environment = request.Environment;

            // Sync Assignees
            ticket.Assignees.Clear();
            foreach (var assigneeId in request.AssigneeIds)
            {
                ticket.Assignees.Add(new TicketAssignee { UserId = assigneeId });
            }

            // Sync Labels
            ticket.Labels.Clear();
            foreach (var label in request.Labels)
            {
                ticket.Labels.Add(new TicketLabel { Label = label });
            }

            await _ticketRepository.UpdateAsync(ticket);
        }

        public async Task UpdateTicketStatusAsync(int id, UpdateTicketStatusRequest request, int userId)
        {
            var ticket = await _ticketRepository.Query().Include(t => t.Status).FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return;

            var newStatus = await _statusRepository.GetByIdAsync(request.StatusId);
            if (newStatus == null) throw new Exception("Status not found");

            // Enforce PauseReason if Paused
            if (newStatus.Name.Equals("Paused", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(request.PauseReason))
            {
                throw new Exception("Pause reason is required");
            }

            // Enforce CancelReason if Cancelled
            if (newStatus.Name.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(request.CancelReason))
            {
                throw new Exception("Cancel reason is required");
            }

            // Record History
            var history = new TicketStatusHistory
            {
                TicketId = id,
                FromStatusId = ticket.StatusId,
                ToStatusId = request.StatusId,
                ChangedByUserId = userId,
                ChangedAt = DateTime.UtcNow
            };
            await _historyRepository.AddAsync(history);

            ticket.StatusId = request.StatusId;
            ticket.PauseReason = request.PauseReason;
            ticket.CancelReason = request.CancelReason;

            if (newStatus.IsTerminal)
            {
                ticket.ActualEndDate = DateTime.UtcNow;
            }
            else
            {
                ticket.ActualEndDate = null;
            }

            await _ticketRepository.UpdateAsync(ticket);
        }

        public async Task AssignTicketAsync(int id, List<int> assigneeIds, int userId)
        {
            var ticket = await _ticketRepository.Query().Include(t => t.Assignees).FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return;

            // Prevent assigning to deactivated users
            var deactivatedUsers = await _projectRepository.Query()
                .SelectMany(p => p.UserProjectRoles)
                .Where(u => assigneeIds.Contains(u.UserId) && !u.User.IsActive)
                .Select(u => u.User.DisplayName)
                .ToListAsync();

            if (deactivatedUsers.Any())
            {
                throw new Exception($"Cannot assign to deactivated users: {string.Join(", ", deactivatedUsers)}");
            }

            ticket.Assignees.Clear();
            foreach (var aId in assigneeIds)
            {
                ticket.Assignees.Add(new TicketAssignee { UserId = aId });
            }
            await _ticketRepository.UpdateAsync(ticket);
            // TODO: Notify previous and new assignees
        }

        public async Task SoftDeleteTicketAsync(int id)
        {
            await _ticketRepository.DeleteAsync(id);
        }

        public async Task BulkUpdateStatusAsync(BulkUpdateStatusRequest request, int userId)
        {
            foreach (var id in request.TicketIds)
            {
                await UpdateTicketStatusAsync(id, new UpdateTicketStatusRequest { StatusId = request.StatusId }, userId);
            }
        }

        public async Task BulkAssignAsync(BulkAssignRequest request, int userId)
        {
            foreach (var id in request.TicketIds)
            {
                await AssignTicketAsync(id, request.AssigneeIds, userId);
            }
        }

        public async Task BulkUpdatePriorityAsync(BulkUpdatePriorityRequest request, int userId)
        {
            foreach (var id in request.TicketIds)
            {
                var ticket = await _ticketRepository.GetByIdAsync(id);
                if (ticket != null)
                {
                    ticket.Priority = request.Priority;
                    await _ticketRepository.UpdateAsync(ticket);
                }
            }
        }

        public async Task LinkTicketsAsync(int id, LinkTicketRequest request)
        {
            var link = new TicketLink
            {
                SourceTicketId = id,
                TargetTicketId = request.TargetTicketId,
                LinkType = request.LinkType
            };
            await _linkRepository.AddAsync(link);
        }

        public async Task RemoveLinkAsync(int id, int linkId)
        {
            await _linkRepository.DeleteAsync(linkId);
        }

        public async Task<IEnumerable<TicketStatusHistoryResponse>> GetTicketHistoryAsync(int id)
        {
            var history = await _historyRepository.Query()
                .Include(h => h.FromStatus)
                .Include(h => h.ToStatus)
                .Include(h => h.ChangedByUser)
                .Where(h => h.TicketId == id)
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync();

            return history.Select(h => new TicketStatusHistoryResponse
            {
                FromStatusId = h.FromStatusId,
                FromStatusName = h.FromStatus.Name,
                ToStatusId = h.ToStatusId,
                ToStatusName = h.ToStatus.Name,
                ChangedByUserId = h.ChangedByUserId,
                ChangedByUserName = h.ChangedByUser.UserName ?? h.ChangedByUserId.ToString(),
                ChangedAt = h.ChangedAt
            });
        }

        public async Task<IEnumerable<TicketStatusResponse>> GetStatusesByProjectIdAsync(int projectId)
        {
            var statuses = await _statusRepository.Query()
                .Where(s => s.ProjectId == projectId)
                .OrderBy(s => s.Order)
                .ToListAsync();

            return statuses.Select(MapStatusToResponse);
        }

        public async Task<TicketStatusResponse> CreateStatusAsync(int projectId, CreateTicketStatusRequest request)
        {
            var status = new TicketStatus
            {
                ProjectId = projectId,
                Name = request.Name,
                Colour = request.Colour,
                Order = request.Order,
                IsDefault = request.IsDefault,
                IsTerminal = request.IsTerminal
            };
            await _statusRepository.AddAsync(status);
            return MapStatusToResponse(status);
        }

        public async Task UpdateStatusAsync(int id, CreateTicketStatusRequest request)
        {
            var status = await _statusRepository.GetByIdAsync(id);
            if (status == null) return;

            status.Name = request.Name;
            status.Colour = request.Colour;
            status.Order = request.Order;
            status.IsDefault = request.IsDefault;
            status.IsTerminal = request.IsTerminal;

            await _statusRepository.UpdateAsync(status);
        }

        public async Task DeleteStatusAsync(int id)
        {
            await _statusRepository.DeleteAsync(id);
        }

        private TicketResponse MapToResponse(Ticket ticket)
        {
            return new TicketResponse
            {
                Id = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                Title = ticket.Title,
                Description = ticket.Description,
                Category = ticket.Category,
                Priority = ticket.Priority,
                ProjectId = ticket.ProjectId,
                ProductId = ticket.ProductId,
                SubProjectId = ticket.SubProjectId,
                StatusId = ticket.StatusId,
                StatusName = ticket.Status?.Name ?? "Unknown",
                TeamId = ticket.TeamId,
                ReporterId = ticket.ReporterId,
                StartDate = ticket.StartDate,
                ExpectedDueDate = ticket.ExpectedDueDate,
                ActualEndDate = ticket.ActualEndDate,
                StoryPoints = ticket.StoryPoints,
                AssigneeIds = ticket.Assignees.Select(a => a.UserId).ToList(),
                Labels = ticket.Labels.Select(l => l.Label).ToList()
            };
        }

        private TicketStatusResponse MapStatusToResponse(TicketStatus status)
        {
            return new TicketStatusResponse
            {
                Id = status.Id,
                Name = status.Name,
                Colour = status.Colour,
                Order = status.Order,
                IsDefault = status.IsDefault,
                IsTerminal = status.IsTerminal
            };
        }
    }
}
