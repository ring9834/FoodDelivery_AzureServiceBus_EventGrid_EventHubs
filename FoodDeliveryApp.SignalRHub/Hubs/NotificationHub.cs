using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace FoodDeliveryApp.SignalRHub.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendToUser(string userId, string method, object data)
        {
            await Clients.User(userId).SendAsync(method, data);
        }

        public async Task SendToGroup(string groupName, string method, object data)
        {
            await Clients.Group(groupName).SendAsync(method, data);
        }

        public async Task SendToAll(string method, object data)
        {
            await Clients.All.SendAsync(method, data);
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("JoinedGroup", groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("LeftGroup", groupName);
        }

        public async Task JoinOrderGroup(string orderId)
        {
            var groupName = $"order-{orderId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("JoinedOrderGroup", orderId);
        }

        public async Task LeaveOrderGroup(string orderId)
        {
            var groupName = $"order-{orderId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("LeftOrderGroup", orderId);
        }

        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
            await Clients.Caller.SendAsync("JoinedAdminGroup");
        }

        public override async Task OnConnectedAsync()
        {
            // You can add custom logic here when a client connects
            // For example, authenticate user and add to user-specific group
            var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            // Clean up logic when client disconnects
            await base.OnDisconnectedAsync(exception);
        }
    }
}
