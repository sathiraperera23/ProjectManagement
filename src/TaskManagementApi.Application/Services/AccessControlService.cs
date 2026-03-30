using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Application.DTOs.Projects;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Application.Services
{
    public class AccessControlService : IAccessControlService
    {
        private readonly IRepository<AccessRule> _ruleRepository;
        private readonly IRepository<AccessRequest> _requestRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRoleService _roleService;
        private readonly INotificationService _notificationService;

        public AccessControlService(
            IRepository<AccessRule> ruleRepository,
            IRepository<AccessRequest> requestRepository,
            IRepository<User> userRepository,
            IRoleService roleService,
            INotificationService notificationService)
        {
            _ruleRepository = ruleRepository;
            _requestRepository = requestRepository;
            _userRepository = userRepository;
            _roleService = roleService;
            _notificationService = notificationService;
        }

        public async Task<bool> CanAccessAsync(int userId, AccessComponentType type, int componentId)
        {
            var level = await GetAccessLevelAsync(userId, type, componentId);
            return level != AccessLevel.NoAccess;
        }

        public async Task<AccessLevel> GetAccessLevelAsync(int userId, AccessComponentType type, int componentId)
        {
            // 1. Get all rules for this component
            var rules = await _ruleRepository.Query()
                .Where(r => r.ComponentType == type && r.ComponentId == componentId)
                .ToListAsync();

            if (!rules.Any()) return AccessLevel.FullAccess; // Default if no rules defined

            // 2. Resolve user attributes
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return AccessLevel.NoAccess;

            // 3. Evaluate rules
            // Order: Role match -> Team match -> Project assignment match
            AccessLevel? result = null;
            bool hasOverride = false;

            foreach (var rule in rules.OrderBy(r => r.ConditionType))
            {
                bool applies = false;
                if (rule.ConditionType == AccessConditionType.Role)
                {
                    // Check user roles
                    applies = true; // Placeholder
                }
                else if (rule.ConditionType == AccessConditionType.ProjectAssignment)
                {
                    applies = user.Id.ToString() == rule.ConditionValue;
                }

                if (applies)
                {
                    if (rule.IsOverride)
                    {
                        result = rule.AccessLevel;
                        hasOverride = true;
                    }
                    else if (!hasOverride)
                    {
                        // Most restrictive wins
                        if (result == null || rule.AccessLevel < result)
                            result = rule.AccessLevel;
                    }
                }
            }

            return result ?? AccessLevel.FullAccess;
        }

        public async Task SubmitAccessRequestAsync(int userId, CreateAccessRuleRequest request, string note)
        {
            var accessRequest = new AccessRequest
            {
                UserId = userId,
                ComponentType = request.ComponentType,
                ComponentId = request.ComponentId,
                RequestedAccessLevel = request.AccessLevel,
                Status = AccessRequestStatus.Pending,
                RequestNote = note,
                RequestedAt = DateTime.UtcNow
            };

            await _requestRepository.AddAsync(accessRequest);

            // Notify PM
            await _notificationService.SendAsync(new NotificationEvent
            {
                Type = NotificationEventType.AccessRequest,
                ReferenceId = accessRequest.Id,
                ReferenceType = "AccessRequest",
                Title = "New Access Request",
                Body = $"User has requested {request.AccessLevel} access to {request.ComponentType} {request.ComponentId}."
            });
        }

        public async Task<IEnumerable<AccessRuleDto>> GetProjectRulesAsync(int projectId)
        {
            var rules = await _ruleRepository.Query().Where(r => r.ProjectId == projectId).ToListAsync();
            return rules.Select(r => new AccessRuleDto { Id = r.Id, ComponentType = r.ComponentType, ComponentId = r.ComponentId, ConditionType = r.ConditionType, ConditionValue = r.ConditionValue, AccessLevel = r.AccessLevel, IsOverride = r.IsOverride });
        }

        public async Task<AccessRuleDto> CreateRuleAsync(int projectId, CreateAccessRuleRequest request, int createdByUserId)
        {
            var rule = new AccessRule
            {
                ProjectId = projectId,
                ComponentType = request.ComponentType,
                ComponentId = request.ComponentId,
                ConditionType = request.ConditionType,
                ConditionValue = request.ConditionValue,
                AccessLevel = request.AccessLevel,
                IsOverride = request.IsOverride,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };
            await _ruleRepository.AddAsync(rule);
            return new AccessRuleDto { Id = rule.Id, ComponentType = rule.ComponentType, ComponentId = rule.ComponentId, ConditionType = rule.ConditionType, ConditionValue = rule.ConditionValue, AccessLevel = rule.AccessLevel, IsOverride = rule.IsOverride };
        }

        public async Task UpdateRuleAsync(int id, CreateAccessRuleRequest request)
        {
            var rule = await _ruleRepository.GetByIdAsync(id);
            if (rule == null) return;
            rule.AccessLevel = request.AccessLevel;
            rule.IsOverride = request.IsOverride;
            await _ruleRepository.UpdateAsync(rule);
        }

        public async Task DeleteRuleAsync(int id) => await _ruleRepository.DeleteAsync(id);
        public async Task<IEnumerable<AccessRequestDto>> GetPendingRequestsAsync(int projectId) => (await _requestRepository.Query().Include(r => r.User).Where(r => r.Status == AccessRequestStatus.Pending).ToListAsync()).Select(r => new AccessRequestDto { Id = r.Id, UserId = r.UserId, UserDisplayName = r.User.DisplayName, ComponentType = r.ComponentType, ComponentId = r.ComponentId, RequestedAccessLevel = r.RequestedAccessLevel, RequestNote = r.RequestNote, RequestedAt = r.RequestedAt });
        public async Task ApproveRequestAsync(int requestId, int reviewerId) { /* Implementation */ }
        public async Task RejectRequestAsync(int requestId, string reason, int reviewerId) { /* Implementation */ }
    }
}
