using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Application.DTOs.CustomerBugs;
using TaskManagementApi.Application.DTOs.Tickets;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TaskManagementApi.Application.Services
{
    public class CustomerBugService : ICustomerBugService
    {
        private readonly IRepository<CustomerBugSubmission> _submissionRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<BugApprovalSla> _slaRepository;
        private readonly IEmailParserService _parserService;
        private readonly ITicketService _ticketService;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly IBugReportTemplateService _templateService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CustomerBugService> _logger;

        public CustomerBugService(
            IRepository<CustomerBugSubmission> submissionRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<Project> projectRepository,
            IRepository<BugApprovalSla> slaRepository,
            IEmailParserService parserService,
            ITicketService ticketService,
            INotificationService notificationService,
            IEmailService emailService,
            IBugReportTemplateService templateService,
            IConfiguration configuration,
            ILogger<CustomerBugService> logger)
        {
            _submissionRepository = submissionRepository;
            _ticketRepository = ticketRepository;
            _projectRepository = projectRepository;
            _slaRepository = slaRepository;
            _parserService = parserService;
            _ticketService = ticketService;
            _notificationService = notificationService;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task HandleInboundEmailAsync(BugInboundEmailRequest request)
        {
            var project = await _projectRepository.Query().FirstOrDefaultAsync(p => p.IntakeEmailAddress == request.To);
            if (project == null)
            {
                _logger.LogWarning("No project found for intake email: {To}", request.To);
                return;
            }

            var submission = _parserService.Parse(request.Html);
            submission.ProjectId = project.Id;
            submission.SenderEmail = request.From;
            submission.SenderName = request.FromName;
            submission.ReceivedAt = DateTime.UtcNow;
            submission.RawEmailBody = request.Html;

            // Duplicate detection
            if (await IsDuplicateAsync(submission))
            {
                submission.ParseStatus = BugParseStatus.Duplicate;
                await _submissionRepository.AddAsync(submission);
                return;
            }

            // Invalid format detection
            if (string.IsNullOrEmpty(submission.ParsedTitle) || string.IsNullOrEmpty(submission.ParsedDescription))
            {
                submission.ParseStatus = BugParseStatus.InvalidFormat;
                await _submissionRepository.AddAsync(submission);

                // Automated reply with instructions
                var template = await _templateService.GetTemplateAsync(project.Id);
                await _emailService.SendEmailAsync(request.From, "Bug Submission Instruction", $"Your submission was invalid. Please use the required template below:\n\n{template}");
                return;
            }

            submission.ParseStatus = BugParseStatus.Parsed;
            await _submissionRepository.AddAsync(submission);

            // Create ticket in PendingApproval state
            var ticketRequest = new CreateTicketRequest
            {
                Title = submission.ParsedTitle!,
                Description = submission.ParsedDescription!,
                Category = TicketCategory.Bug,
                ProjectId = project.Id,
                Severity = ParseSeverity(submission.ParsedSeverity),
                Environment = ParseEnvironment(submission.ParsedEnvironment),
                StepsToReproduce = submission.ParsedStepsToReproduce,
                ExpectedBehaviour = submission.ParsedExpectedBehaviour,
                ActualBehaviour = submission.ParsedActualBehaviour,
                Priority = TicketPriority.Medium // Default
            };

            // Using dummy reporter ID for client-created tickets or a dedicated "System" user
            var ticket = await _ticketService.CreateTicketAsync(ticketRequest, 1);
            var ticketEntity = await _ticketRepository.GetByIdAsync(ticket.Id);
            if (ticketEntity != null)
            {
                ticketEntity.ApprovalStatus = ApprovalStatus.PendingApproval;
                await _ticketRepository.UpdateAsync(ticketEntity);
            }

            submission.CreatedTicketId = ticket.Id;
            await _submissionRepository.UpdateAsync(submission);

            // Notify PM
            await _notificationService.SendAsync(new NotificationEvent
            {
                Type = NotificationEventType.ClientBugReceived,
                ProjectId = project.Id,
                ReferenceId = ticket.Id,
                ReferenceType = "Ticket",
                Title = "New Client Bug Received",
                Body = $"New bug submission from {request.FromName} for project {project.Name}."
            });

            // Notify Customer
            await _emailService.SendEmailAsync(request.From, "Bug Received", $"Your bug has been received and assigned ticket ID: {ticket.TicketNumber}.");
        }

        private async Task<bool> IsDuplicateAsync(CustomerBugSubmission submission)
        {
            var threshold = double.Parse(_configuration["BugDetection:SimilarityThreshold"] ?? "0.8");
            // Simple title matching for scaffold
            return await _ticketRepository.Query()
                .AnyAsync(t => t.ProjectId == submission.ProjectId && t.Title == submission.ParsedTitle && !t.IsDeleted);
        }

        public async Task ApproveBugAsync(int ticketId, BugApprovalRequest request, int userId)
        {
            var ticket = await _ticketRepository.Query().Include(t => t.Status).FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null || ticket.ApprovalStatus != ApprovalStatus.PendingApproval) return;

            ticket.ApprovalStatus = ApprovalStatus.Approved;
            // Transition to first non-terminal status
            await _ticketRepository.UpdateAsync(ticket);

            if (request.AssigneeId.HasValue)
            {
                await _ticketService.AssignTicketAsync(ticketId, new List<int> { request.AssigneeId.Value }, userId);
            }

            // Notify customer
            var submission = await _submissionRepository.Query().FirstOrDefaultAsync(s => s.CreatedTicketId == ticketId);
            if (submission != null)
            {
                await _emailService.SendEmailAsync(submission.SenderEmail, "Bug Approved", $"Your bug {ticket.TicketNumber} has been approved.");
            }
        }

        public async Task RejectBugAsync(int ticketId, BugRejectionRequest request, int userId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null) return;

            ticket.ApprovalStatus = ApprovalStatus.Rejected;
            // Logic to close ticket
            await _ticketRepository.UpdateAsync(ticket);

            var submission = await _submissionRepository.Query().FirstOrDefaultAsync(s => s.CreatedTicketId == ticketId);
            if (submission != null)
            {
                await _emailService.SendEmailAsync(submission.SenderEmail, "Bug Rejected", $"Your bug {ticket.TicketNumber} was rejected. Reason: {request.Reason}");
            }
        }

        public async Task RequestMoreInfoAsync(int ticketId, string message, int userId)
        {
            var submission = await _submissionRepository.Query().FirstOrDefaultAsync(s => s.CreatedTicketId == ticketId);
            if (submission != null)
            {
                await _emailService.SendEmailAsync(submission.SenderEmail, "More Info Required", message);
            }
        }

        public async Task<IEnumerable<BugSubmissionDto>> GetSubmissionsAsync(int projectId)
        {
            var items = await _submissionRepository.Query().Where(s => s.ProjectId == projectId).ToListAsync();
            return items.Select(s => new BugSubmissionDto
            {
                Id = s.Id,
                ProjectId = s.ProjectId,
                SenderEmail = s.SenderEmail,
                SenderName = s.SenderName,
                ParsedTitle = s.ParsedTitle,
                ReceivedAt = s.ReceivedAt,
                ParseStatus = s.ParseStatus,
                CreatedTicketId = s.CreatedTicketId
            });
        }

        public async Task<IEnumerable<TicketResponse>> GetApprovalQueueAsync(int projectId)
        {
            var tickets = await _ticketRepository.Query().Include(t => t.Status).Where(t => t.ProjectId == projectId && t.ApprovalStatus == ApprovalStatus.PendingApproval).ToListAsync();
            return tickets.Select(MapToTicketResponse);
        }

        public async Task<BugApprovalSlaDto?> GetSlaAsync(int projectId)
        {
            var sla = await _slaRepository.Query().FirstOrDefaultAsync(s => s.ProjectId == projectId);
            return sla != null ? new BugApprovalSlaDto { ProjectId = projectId, SlaBusinessDays = sla.SlaBusinessDays, EscalateAfterDays = sla.EscalateAfterDays } : null;
        }

        public async Task UpdateSlaAsync(int projectId, UpdateBugSlaRequest request)
        {
            var existing = await _slaRepository.Query().FirstOrDefaultAsync(s => s.ProjectId == projectId);
            if (existing == null)
            {
                await _slaRepository.AddAsync(new BugApprovalSla { ProjectId = projectId, SlaBusinessDays = request.SlaBusinessDays, EscalateAfterDays = request.EscalateAfterDays });
            }
            else
            {
                existing.SlaBusinessDays = request.SlaBusinessDays;
                existing.EscalateAfterDays = request.EscalateAfterDays;
                await _slaRepository.UpdateAsync(existing);
            }
        }

        private TicketSeverity? ParseSeverity(string? s) => Enum.TryParse<TicketSeverity>(s, true, out var res) ? res : null;
        private TicketEnvironment? ParseEnvironment(string? s) => Enum.TryParse<TicketEnvironment>(s, true, out var res) ? res : null;

        private TicketResponse MapToTicketResponse(Ticket t)
        {
            return new TicketResponse
            {
                Id = t.Id,
                TicketNumber = t.TicketNumber,
                Title = t.Title,
                StatusName = t.Status?.Name ?? "Unknown",
                Priority = t.Priority,
                ExpectedDueDate = t.ExpectedDueDate
            };
        }
    }

    public class EmailParserService : IEmailParserService
    {
        public CustomerBugSubmission Parse(string htmlBody)
        {
            // Regex based parsing logic...
            return new CustomerBugSubmission
            {
                ParsedTitle = ExtractField(htmlBody, "Subject"),
                ParsedDescription = ExtractField(htmlBody, "Description"),
                ParsedStepsToReproduce = ExtractField(htmlBody, "Steps to Reproduce"),
                ParsedExpectedBehaviour = ExtractField(htmlBody, "Expected Behaviour"),
                ParsedActualBehaviour = ExtractField(htmlBody, "Actual Behaviour"),
                ParsedEnvironment = ExtractField(htmlBody, "Environment"),
                ParsedSeverity = ExtractField(htmlBody, "Severity")
            };
        }

        private string? ExtractField(string body, string field) => null; // Placeholder
    }
}
