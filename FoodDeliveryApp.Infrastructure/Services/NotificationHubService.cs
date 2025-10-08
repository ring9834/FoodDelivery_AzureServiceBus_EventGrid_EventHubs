using FoodDeliveryApp.Core.Interfaces;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoodDeliveryApp.Infrastructure.Services
{
    public class NotificationHubService : INotificationHubService
    {
        private readonly NotificationHubClient _hubClient;
        private readonly ILogger<NotificationHubService> _logger;

        public NotificationHubService(
            IConfiguration configuration,
            ILogger<NotificationHubService> logger)
        {
            var connectionString = configuration["NotificationHub:ConnectionString"];
            var hubName = configuration["NotificationHub:HubName"];

            _hubClient = NotificationHubClient.CreateClientFromConnectionString(
                connectionString,
                hubName);

            _logger = logger;
        }

        public async Task SendPushNotificationAsync(
            string userId,
            string title,
            string message,
            Dictionary<string, string>? data = null)
        {
            try
            {
                // Android (FCM) notification
                var androidPayload = CreateAndroidPayload(title, message, data);
                await _hubClient.SendFcmNativeNotificationAsync(androidPayload, userId);

                // iOS (APNs) notification
                var iosPayload = CreateiOSPayload(title, message, data);
                await _hubClient.SendAppleNativeNotificationAsync(iosPayload, userId);

                _logger.LogInformation(
                    "Sent push notification to user {UserId}",
                    userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send push notification to user {UserId}",
                    userId);
                throw;
            }
        }

        public async Task SendPushNotificationToTagAsync(
            string tag,
            string title,
            string message,
            Dictionary<string, string>? data = null)
        {
            try
            {
                // Android (FCM) notification
                var androidPayload = CreateAndroidPayload(title, message, data);
                await _hubClient.SendFcmNativeNotificationAsync(androidPayload, tag);

                // iOS (APNs) notification
                var iosPayload = CreateiOSPayload(title, message, data);
                await _hubClient.SendAppleNativeNotificationAsync(iosPayload, tag);

                _logger.LogInformation(
                    "Sent push notification to tag {Tag}",
                    tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send push notification to tag {Tag}",
                    tag);
                throw;
            }
        }

        private string CreateAndroidPayload(string title, string message, Dictionary<string, string>? data)
        {
            var payload = new
            {
                notification = new
                {
                    title,
                    body = message
                },
                data = data ?? new Dictionary<string, string>()
            };

            return System.Text.Json.JsonSerializer.Serialize(payload);
        }

        private string CreateiOSPayload(string title, string message, Dictionary<string, string>? data)
        {
            var payload = new
            {
                aps = new
                {
                    alert = new
                    {
                        title,
                        body = message
                    },
                    sound = "default"
                },
                data = data ?? new Dictionary<string, string>()
            };

            return System.Text.Json.JsonSerializer.Serialize(payload);
        }
    }
}