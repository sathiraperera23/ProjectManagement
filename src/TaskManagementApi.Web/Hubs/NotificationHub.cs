using Microsoft.AspNetCore.SignalR;

namespace TaskManagementApi.Web.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            // Join a group based on UserId
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.UserIdentifier;
            if (userId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            }
            await base.OnConnectedAsync();
        }

        public async Task MarkAsRead(int notificationId)
        {
            // Optional: Notify other client instances of this user
        }
    }

    public interface INotificationHubClient
    {
        Task ReceiveNotification(object notification);
    }
}
