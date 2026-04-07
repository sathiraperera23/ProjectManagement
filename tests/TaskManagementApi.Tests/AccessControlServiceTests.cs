using Moq;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Application.Services;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace TaskManagementApi.Tests
{
    public class AccessControlServiceTests
    {
        private readonly Mock<IRepository<AccessRule>> _ruleRepo;
        private readonly Mock<IRepository<AccessRequest>> _requestRepo;
        private readonly Mock<IRepository<User>> _userRepo;
        private readonly Mock<IRoleService> _roleService;
        private readonly Mock<INotificationService> _notifService;
        private readonly AccessControlService _service;

        public AccessControlServiceTests()
        {
            _ruleRepo = new Mock<IRepository<AccessRule>>();
            _requestRepo = new Mock<IRepository<AccessRequest>>();
            _userRepo = new Mock<IRepository<User>>();
            _roleService = new Mock<IRoleService>();
            _notifService = new Mock<INotificationService>();

            _service = new AccessControlService(
                _ruleRepo.Object, _requestRepo.Object, _userRepo.Object,
                _roleService.Object, _notifService.Object);
        }

        [Fact]
        public async Task GetAccessLevel_MostRestrictiveRuleWins_UnlessOverride()
        {
            // Arrange
            var userId = 1;
            var compId = 10;
            var type = AccessComponentType.Project;

            var user = new User { Id = userId };
            _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

            var rules = new List<AccessRule>
            {
                new AccessRule { ComponentType = type, ComponentId = compId, AccessLevel = AccessLevel.Edit, ConditionType = AccessConditionType.ProjectAssignment, ConditionValue = userId.ToString() },
                new AccessRule { ComponentType = type, ComponentId = compId, AccessLevel = AccessLevel.ViewOnly, ConditionType = AccessConditionType.ProjectAssignment, ConditionValue = userId.ToString() }
            };

            _ruleRepo.SetupAsyncQueryable(rules.AsQueryable());

            // Act
            var result = await _service.GetAccessLevelAsync(userId, type, compId);

            // Assert
            Assert.Equal(AccessLevel.ViewOnly, result); // Most restrictive (ViewOnly < Edit)
        }

        [Fact]
        public async Task GetAccessLevel_OverrideBypassesRestriction()
        {
            // Arrange
            var userId = 1;
            var rules = new List<AccessRule>
            {
                new AccessRule { ComponentType = AccessComponentType.Project, ComponentId = 10, AccessLevel = AccessLevel.NoAccess, ConditionType = AccessConditionType.ProjectAssignment, ConditionValue = userId.ToString() },
                new AccessRule { ComponentType = AccessComponentType.Project, ComponentId = 10, AccessLevel = AccessLevel.FullAccess, IsOverride = true, ConditionType = AccessConditionType.ProjectAssignment, ConditionValue = userId.ToString() }
            };

            _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User { Id = userId });
            _ruleRepo.SetupAsyncQueryable(rules.AsQueryable());

            // Act
            var result = await _service.GetAccessLevelAsync(userId, AccessComponentType.Project, 10);

            // Assert
            Assert.Equal(AccessLevel.FullAccess, result);
        }
    }
}
