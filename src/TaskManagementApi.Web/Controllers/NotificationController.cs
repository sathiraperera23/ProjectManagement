using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Application.DTOs.Notifications;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Constants;
using TaskManagementApi.Web.Authorization;
using System.Security.Claims;

namespace TaskManagementApi.Web.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IUserManagerFacade _userManager;

        public NotificationController(INotificationService notificationService, IUserManagerFacade userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        private async Task<int> GetCurrentUserId()
        {
            var providerId = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (providerId == null) return 0;
            var user = await _userManager.FindByProviderIdAsync(providerId);
            return user?.Id ?? 0;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var notifications = await _notificationService.GetUserNotificationsAsync(await GetCurrentUserId(), page, pageSize);
            return Ok(notifications);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await _notificationService.GetUnreadCountAsync(await GetCurrentUserId());
            return Ok(new { count });
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkAsReadAsync(id, await GetCurrentUserId());
            return NoContent();
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await _notificationService.MarkAllAsReadAsync(await GetCurrentUserId());
            return NoContent();
        }

        [HttpGet("preferences")]
        public async Task<IActionResult> GetPreferences()
        {
            var preferences = await _notificationService.GetUserPreferencesAsync(await GetCurrentUserId());
            return Ok(preferences);
        }

        [HttpPut("preferences")]
        public async Task<IActionResult> UpdatePreferences([FromBody] UpdateNotificationPreferenceRequest request)
        {
            await _notificationService.UpdateUserPreferencesAsync(await GetCurrentUserId(), request);
            return NoContent();
        }

        [HttpGet("/api/projects/{projectId}/notification-rules")]
        [RequirePermission(Permissions.ManageNotificationSettings)]
        public async Task<IActionResult> GetProjectRules(int projectId)
        {
            var rules = await _notificationService.GetProjectRulesAsync(projectId);
            return Ok(rules);
        }

        [HttpGet("log")]
        [RequirePermission(Permissions.ManageNotificationSettings)]
        public async Task<IActionResult> GetLog([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var logs = await _notificationService.GetNotificationLogsAsync(page, pageSize);
            return Ok(logs);
        }
    }
}
