using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using Moq;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Application.Services;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace TaskManagementApi.Tests
{
    public class NotificationServiceTests
    {
        private readonly Mock<IRepository<Notification>> _notificationRepoMock;
        private readonly Mock<IRepository<NotificationPreference>> _preferenceRepoMock;
        private readonly Mock<IRepository<NotificationLog>> _logRepoMock;
        private readonly Mock<IRepository<NotificationRule>> _ruleRepoMock;
        private readonly Mock<IRepository<User>> _userRepoMock;
        private readonly Mock<IEmailService> _emailMock;
        private readonly Mock<ISmsService> _smsMock;
        private readonly Mock<INotificationHubService> _hubMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly NotificationService _service;

        public NotificationServiceTests()
        {
            _notificationRepoMock = new Mock<IRepository<Notification>>();
            _preferenceRepoMock = new Mock<IRepository<NotificationPreference>>();
            _logRepoMock = new Mock<IRepository<NotificationLog>>();
            _ruleRepoMock = new Mock<IRepository<NotificationRule>>();
            _userRepoMock = new Mock<IRepository<User>>();
            _emailMock = new Mock<IEmailService>();
            _smsMock = new Mock<ISmsService>();
            _hubMock = new Mock<INotificationHubService>();
            _configMock = new Mock<IConfiguration>();

            _service = new NotificationService(
                _notificationRepoMock.Object, _preferenceRepoMock.Object, _logRepoMock.Object,
                _ruleRepoMock.Object, _userRepoMock.Object, _emailMock.Object, _smsMock.Object,
                _hubMock.Object, _configMock.Object, new Mock<ILogger<NotificationService>>().Object);
        }

        [Fact]
        public async Task SendAsync_SuppressesEscalation_WhenRevisedDueDateExists()
        {
            // Arrange
            var e = new NotificationEvent
            {
                Type = NotificationEventType.TicketOverdue,
                ReferenceId = 1,
                RevisedDueDate = DateTime.Now.AddDays(1)
            };

            // Act
            await _service.SendAsync(e);

            // Assert
            _emailMock.Verify(m => m.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SendAsync_RespectsPersonalQuietHours()
        {
            // Arrange
            var userId = 1;
            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                QuietHourStart = TimeSpan.FromHours(20), // 8 PM
                QuietHourEnd = TimeSpan.FromHours(8)      // 8 AM
            };

            var e = new NotificationEvent { Type = NotificationEventType.TicketAssigned, SpecificUserId = userId };

            _userRepoMock.SetupAsyncQueryable(new List<User> { user }.AsQueryable());
            _preferenceRepoMock.SetupAsyncQueryable(new List<NotificationPreference>().AsQueryable());
            _ruleRepoMock.SetupAsyncQueryable(new List<NotificationRule>().AsQueryable());

            // Mock current time as inside quiet hours
            var currentUtc = DateTime.UtcNow.TimeOfDay;
            user.QuietHourStart = currentUtc.Subtract(TimeSpan.FromHours(1));
            user.QuietHourEnd = currentUtc.Add(TimeSpan.FromHours(1));

            // Act
            await _service.SendAsync(e);

            // Assert
            _emailMock.Verify(m => m.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SendAsync_SuppressesSms_DuringGlobalQuietHours()
        {
            // Arrange
            var userId = 1;
            var user = new User { Id = userId, MobileNumber = "12345", MobileVerified = true };
            _configMock.Setup(c => c["Org:SmsQuietHours:Start"]).Returns("00:00");
            _configMock.Setup(c => c["Org:SmsQuietHours:End"]).Returns("23:59"); // Always quiet

            var e = new NotificationEvent { Type = NotificationEventType.TicketAssigned, SpecificUserId = userId };
            _userRepoMock.SetupAsyncQueryable(new List<User> { user }.AsQueryable());
            _ruleRepoMock.SetupAsyncQueryable(new List<NotificationRule>().AsQueryable());

            // Force SMS preference
            var pref = new List<NotificationPreference> { new NotificationPreference { UserId = userId, EventType = e.Type, Sms = true } };
            _preferenceRepoMock.SetupAsyncQueryable(pref.AsQueryable());

            // Act
            await _service.SendAsync(e);

            // Assert
            _smsMock.Verify(m => m.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SendAsync_ResolvesProjectPreferenceOverGlobal()
        {
            // Arrange
            var userId = 1;
            var projectId = 10;
            var user = new User { Id = userId, Email = "test@example.com" };
            var e = new NotificationEvent
            {
                Type = NotificationEventType.TicketAssigned,
                ProjectId = projectId,
                SpecificUserId = userId
            };

            _userRepoMock.SetupAsyncQueryable(new List<User> { user }.AsQueryable());
            _ruleRepoMock.SetupAsyncQueryable(new List<NotificationRule>().AsQueryable());

            // Global: Email ON
            var globalPref = new NotificationPreference { UserId = userId, ProjectId = null, EventType = e.Type, Email = true };
            // Project: Email OFF
            var projectPref = new NotificationPreference { UserId = userId, ProjectId = projectId, EventType = e.Type, Email = false };

            _preferenceRepoMock.SetupAsyncQueryable(new List<NotificationPreference> { globalPref, projectPref }.AsQueryable());

            // Act
            await _service.SendAsync(e);

            // Assert
            _emailMock.Verify(m => m.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
