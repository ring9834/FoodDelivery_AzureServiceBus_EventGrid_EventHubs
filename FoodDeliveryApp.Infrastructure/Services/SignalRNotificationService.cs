using FoodDeliveryApp.Core.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FoodDeliveryApp.Infrastructure.Services
{
    public class SignalRNotificationService : ISignalRNotificationService
    {
        private readonly HubConnection _hubConnection;
        private readonly ILogger<SignalRNotificationService> _logger;

        public SignalRNotificationService(
            IConfiguration configuration,
            ILogger<SignalRNotificationService> logger)
        {
            var signalRUrl = configuration["SignalR:HubUrl"];
            var accessKey = configuration["SignalR:AccessKey"];

            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{signalRUrl}/notifications")
                .WithAutomaticReconnect()
                .Build();

            _logger = logger;

            // Start connection
            Task.Run(async () =>
            {
                try
                {
                    await _hubConnection.StartAsync();
                    _logger.LogInformation("SignalR connection started");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error starting SignalR connection");
                }
            });
        }

        public async Task SendToUserAsync(string userId, string method, object data)
        {
            try
            {
                await EnsureConnected();
                await _hubConnection.InvokeAsync("SendToUser", userId, method, data);

                _logger.LogInformation(
                    "Sent SignalR message to user {UserId} with method {Method}",
                    userId,
                    method);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send SignalR message to user {UserId}",
                    userId);
                throw;
            }
        }

        public async Task SendToGroupAsync(string groupName, string method, object data)
        {
            try
            {
                await EnsureConnected();
                await _hubConnection.InvokeAsync("SendToGroup", groupName, method, data);

                _logger.LogInformation(
                    "Sent SignalR message to group {GroupName} with method {Method}",
                    groupName,
                    method);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send SignalR message to group {GroupName}",
                    groupName);
                throw;
            }
        }

        public async Task SendToAllAsync(string method, object data)
        {
            try
            {
                await EnsureConnected();
                await _hubConnection.InvokeAsync("SendToAll", method, data);

                _logger.LogInformation(
                    "Sent SignalR message to all with method {Method}",
                    method);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SignalR message to all");
                throw;
            }
        }

        public async Task AddToGroupAsync(string connectionId, string groupName)
        {
            try
            {
                await EnsureConnected();
                await _hubConnection.InvokeAsync("AddToGroup", connectionId, groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to add connection {ConnectionId} to group {GroupName}",
                    connectionId,
                    groupName);
                throw;
            }
        }

        public async Task RemoveFromGroupAsync(string connectionId, string groupName)
        {
            try
            {
                await EnsureConnected();
                await _hubConnection.InvokeAsync("RemoveFromGroup", connectionId, groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to remove connection {ConnectionId} from group {GroupName}",
                    connectionId,
                    groupName);
                throw;
            }
        }

        private async Task EnsureConnected()
        {
            if (_hubConnection.State != HubConnectionState.Connected)
            {
                await _hubConnection.StartAsync();
            }
        }
    }
}