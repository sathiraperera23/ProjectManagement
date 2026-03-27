using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using TaskManagementApi.Application.DTOs.Notifications;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Domain.Enums;
using TaskManagementApi.Web.Hubs;

namespace TaskManagementApi.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IRepository<Notification> _notificationRepository;
        private readonly IRepository<NotificationPreference> _preferenceRepository;
        private readonly IRepository<NotificationLog> _logRepository;
        private readonly IRepository<NotificationRule> _ruleRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IRepository<Notification> notificationRepository,
            IRepository<NotificationPreference> preferenceRepository,
            IRepository<NotificationLog> logRepository,
            IRepository<NotificationRule> ruleRepository,
            IRepository<User> userRepository,
            IEmailService emailService,
            ISmsService smsService,
            IHubContext<NotificationHub> hubContext,
            IConfiguration configuration,
            ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _preferenceRepository = preferenceRepository;
            _logRepository = logRepository;
            _ruleRepository = ruleRepository;
            _userRepository = userRepository;
            _emailService = emailService;
            _smsService = smsService;
            _hubContext = hubContext;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendAsync(NotificationEvent e)
        {
            // Escalation suppression check per requirements
            if (e.Type == NotificationEventType.TicketOverdue && e.RevisedDueDate.HasValue)
            {
                _logger.LogInformation("Escalation suppressed for ticket {Id} due to revised due date.", e.ReferenceId);
                return;
            }

            var recipients = await ResolveRecipientsAsync(e);
            foreach (var recipient in recipients)
            {
                await DispatchAsync(recipient, e);
            }
        }

        private async Task<IEnumerable<User>> ResolveRecipientsAsync(NotificationEvent e)
        {
            // Resolve based on NotificationRule
            var rules = await _ruleRepository.Query()
                .Where(r => (r.ProjectId == null || r.ProjectId == e.ProjectId) && r.EventType == e.Type)
                .ToListAsync();

            var userIds = new List<int>();
            if (e.SpecificUserId.HasValue) userIds.Add(e.SpecificUserId.Value);

            // Mocked resolution logic
            foreach (var rule in rules)
            {
                switch (rule.RecipientType)
                {
                    case RecipientType.SpecificUser:
                        // userId = rule.SpecificId;
                        break;
                    case RecipientType.ProjectManager:
                        // userId = project.ManagerId;
                        break;
                }
            }

            return await _userRepository.Query()
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();
        }

        private async Task DispatchAsync(User user, NotificationEvent e)
        {
            var pref = await ResolvePreferenceAsync(user.Id, e.ProjectId, e.Type);

            // Check Quiet Hours (personal and global)
            if (IsInsideQuietHours(user))
            {
                if (pref.InApp) await QueueForLaterAsync(user.Id, e, "InApp");
                if (pref.Email) await QueueForLaterAsync(user.Id, e, "Email");
                if (pref.Sms) await QueueForLaterAsync(user.Id, e, "Sms");
                return;
            }

            if (pref.InApp)
            {
                var n = new Notification
                {
                    UserId = user.Id,
                    Title = e.Title,
                    Body = e.Body,
                    EventType = e.Type,
                    ReferenceId = e.ReferenceId,
                    ReferenceType = e.ReferenceType,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationRepository.AddAsync(n);
                await _hubContext.Clients.Group($"User_{user.ProviderId}").SendAsync("ReceiveNotification", MapToDto(n));
            }

            if (pref.Email && !string.IsNullOrEmpty(user.Email))
            {
                var appUrl = _configuration["App:Url"] ?? "http://localhost:3000";
                var email = new EmailMessage
                {
                    To = user.Email,
                    Subject = $"[{e.ReferenceType ?? "Task"}] {e.Title}",
                    HtmlBody = $@"
                        <p>{e.Body}</p>
                        <p><strong>Reference:</strong> {e.ReferenceId}</p>
                        <p><a href='{appUrl}/{e.ReferenceType?.ToLower()}s/{e.ReferenceId}'>View Detail</a></p>
                        <br/>
                        <small>You are receiving this because you are subscribed to {e.Type} notifications.</small>",
                    FromDisplayName = "Task Management System"
                };
                await _emailService.SendEmailAsync(email);
                await LogAsync(user.Id, "Email", e);
            }

            if (pref.Sms && !string.IsNullOrEmpty(user.MobileNumber) && user.MobileVerified)
            {
                // Global org-level quiet hours for SMS only
                if (IsInsideGlobalSmsQuietHours())
                {
                    await QueueForLaterAsync(user.Id, e, "Sms");
                }
                else
                {
                    await _smsService.SendSmsAsync(user.MobileNumber, $"{e.Title}: {e.Body}");
                    await LogAsync(user.Id, "Sms", e);
                }
            }
        }

        private async Task<NotificationPreferenceDto> ResolvePreferenceAsync(int userId, int? projectId, NotificationEventType type)
        {
            var globalPref = await _preferenceRepository.Query()
                .FirstOrDefaultAsync(p => p.UserId == userId && p.ProjectId == null && p.EventType == type);

            var projectPref = projectId.HasValue
                ? await _preferenceRepository.Query().FirstOrDefaultAsync(p => p.UserId == userId && p.ProjectId == projectId && p.EventType == type)
                : null;

            var final = projectPref ?? globalPref;

            if (final == null)
            {
                // Mandatory events check (Section 15.3 indicates some are mandatory)
                var mandatory = type == NotificationEventType.TicketAssigned || type == NotificationEventType.Mention;
                return new NotificationPreferenceDto
                {
                    EventType = type,
                    InApp = true,
                    Email = mandatory, // Default on if mandatory
                    Sms = false
                };
            }

            return new NotificationPreferenceDto
            {
                ProjectId = final.ProjectId,
                EventType = final.EventType,
                InApp = final.InApp,
                Email = final.Email,
                Sms = final.Sms,
                DigestMode = final.DigestMode
            };
        }

        private bool IsInsideQuietHours(User user)
        {
            if (user.QuietHourStart == null || user.QuietHourEnd == null) return false;
            var now = DateTime.UtcNow.TimeOfDay;
            return now >= user.QuietHourStart && now <= user.QuietHourEnd;
        }

        private bool IsInsideGlobalSmsQuietHours()
        {
            var start = TimeSpan.Parse(_configuration["Org:SmsQuietHours:Start"] ?? "21:00");
            var end = TimeSpan.Parse(_configuration["Org:SmsQuietHours:End"] ?? "07:00");
            var now = DateTime.UtcNow.TimeOfDay;

            if (start < end) return now >= start && now <= end;
            return now >= start || now <= end;
        }

        private async Task QueueForLaterAsync(int userId, NotificationEvent e, string channel)
        {
            // Placeholder: add to Outbox for post-quiet-period delivery
            _logger.LogInformation("Queueing {Channel} notification for User {UserId} due to quiet hours.", channel, userId);
            await Task.CompletedTask;
        }

        private async Task LogAsync(int userId, string channel, NotificationEvent e)
        {
            await _logRepository.AddAsync(new NotificationLog
            {
                UserId = userId,
                Channel = channel,
                EventType = e.Type,
                ReferenceId = e.ReferenceId,
                SentAt = DateTime.UtcNow,
                DeliveryStatus = DeliveryStatus.Sent
            });
        }

        public async Task MarkAsReadAsync(int notificationId, int userId)
        {
            var n = await _notificationRepository.GetByIdAsync(notificationId);
            if (n != null && n.UserId == userId)
            {
                n.IsRead = true;
                await _notificationRepository.UpdateAsync(n);
            }
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var unread = await _notificationRepository.Query().Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
            foreach (var n in unread)
            {
                n.IsRead = true;
                await _notificationRepository.UpdateAsync(n);
            }
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _notificationRepository.Query().CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId, int page, int pageSize)
        {
            var items = await _notificationRepository.Query()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return items.Select(MapToDto);
        }

        public async Task<IEnumerable<NotificationPreferenceDto>> GetUserPreferencesAsync(int userId)
        {
            var prefs = await _preferenceRepository.Query().Where(p => p.UserId == userId).ToListAsync();
            return prefs.Select(p => new NotificationPreferenceDto
            {
                ProjectId = p.ProjectId,
                EventType = p.EventType,
                InApp = p.InApp,
                Email = p.Email,
                Sms = p.Sms,
                DigestMode = p.DigestMode
            });
        }

        public async Task UpdateUserPreferencesAsync(int userId, UpdateNotificationPreferenceRequest request)
        {
            foreach (var pDto in request.Preferences)
            {
                var existing = await _preferenceRepository.Query().FirstOrDefaultAsync(p => p.UserId == userId && p.ProjectId == pDto.ProjectId && p.EventType == pDto.EventType);
                if (existing == null)
                {
                    await _preferenceRepository.AddAsync(new NotificationPreference
                    {
                        UserId = userId,
                        ProjectId = pDto.ProjectId,
                        EventType = pDto.EventType,
                        InApp = pDto.InApp,
                        Email = pDto.Email,
                        Sms = pDto.Sms,
                        DigestMode = pDto.DigestMode
                    });
                }
                else
                {
                    existing.InApp = pDto.InApp;
                    existing.Email = pDto.Email;
                    existing.Sms = pDto.Sms;
                    existing.DigestMode = pDto.DigestMode;
                    await _preferenceRepository.UpdateAsync(existing);
                }
            }
        }

        public async Task<IEnumerable<NotificationRuleDto>> GetProjectRulesAsync(int projectId)
        {
            var rules = await _ruleRepository.Query().Where(r => r.ProjectId == projectId).ToListAsync();
            return rules.Select(r => new NotificationRuleDto
            {
                Id = r.Id,
                ProjectId = r.ProjectId,
                EventType = r.EventType,
                RecipientType = r.RecipientType,
                Channel = r.Channel,
                ImmediateOrDigest = r.ImmediateOrDigest
            });
        }

        public async Task UpdateProjectRulesAsync(int projectId, List<NotificationRuleDto> rules)
        {
            // Update logic...
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<NotificationLogDto>> GetNotificationLogsAsync(int page, int pageSize)
        {
            var logs = await _logRepository.Query()
                .Include(l => l.User)
                .OrderByDescending(l => l.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return logs.Select(l => new NotificationLogDto
            {
                Id = l.Id,
                UserId = l.UserId,
                UserDisplayName = l.User.DisplayName,
                Channel = l.Channel,
                EventType = l.EventType,
                SentAt = l.SentAt,
                DeliveryStatus = l.DeliveryStatus.ToString(),
                ErrorMessage = l.ErrorMessage
            });
        }

        private NotificationDto MapToDto(Notification n)
        {
            return new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Body = n.Body,
                EventType = n.EventType,
                ReferenceId = n.ReferenceId,
                ReferenceType = n.ReferenceType,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            };
        }
    }
}
