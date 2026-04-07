using Microsoft.AspNetCore.SignalR;
using TaskManagementApi.Application.DTOs.Notifications;
using TaskManagementApi.Application.Interfaces;

namespace TaskManagementApi.Web.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.UserIdentifier;
            if (userId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            }
            await base.OnConnectedAsync();
        }
    }

    public class NotificationHubService : INotificationHubService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationHubService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationAsync(string providerId, NotificationDto notification)
        {
            await _hubContext.Clients.Group($"User_{providerId}").SendAsync("ReceiveNotification", notification);
        }
    }
}
