using Azure.Messaging.EventGrid;
using FoodDeliveryApp.Core.Interfaces;
using FoodDeliveryApp.Domain.Events;
using FoodDeliveryApp.Domain.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace FoodDeliveryApp.Functions.Functions
{
    public class OrderUpdatedFunction
    {
        private readonly ISignalRNotificationService _signalRService;
        private readonly INotificationHubService _notificationHubService;
        private readonly ILogger<OrderUpdatedFunction> _logger;

        public OrderUpdatedFunction(
            ISignalRNotificationService signalRService,
            INotificationHubService notificationHubService,
            ILogger<OrderUpdatedFunction> logger)
        {
            _signalRService = signalRService;
            _notificationHubService = notificationHubService;
            _logger = logger;
        }

        [Function("OrderUpdatedFunction")]
        public async Task Run(
            [EventGridTrigger] EventGridEvent eventGridEvent)
        {
            try
            {
                _logger.LogInformation("Processing OrderUpdated event: {EventId}", eventGridEvent.Id);

                var orderUpdatedEvent = JsonSerializer.Deserialize<OrderUpdatedEvent>(
                    eventGridEvent.Data.ToString());

                // Send real-time notification to customer via SignalR
                await _signalRService.SendToUserAsync(
                    orderUpdatedEvent.CustomerId,
                    "OrderStatusUpdated",
                    new
                    {
                        orderId = orderUpdatedEvent.OrderId,
                        oldStatus = orderUpdatedEvent.OldStatus.ToString(),
                        newStatus = orderUpdatedEvent.NewStatus.ToString()
                    });

                // Send push notification for important status changes
                if (ShouldSendPushNotification(orderUpdatedEvent.NewStatus))
                {
                    var message = GetStatusMessage(orderUpdatedEvent.NewStatus);
                    await _notificationHubService.SendPushNotificationAsync(
                        orderUpdatedEvent.CustomerId,
                        "Order Update",
                        message,
                        new System.Collections.Generic.Dictionary<string, string>
                        {
                            { "orderId", orderUpdatedEvent.OrderId },
                            { "status", orderUpdatedEvent.NewStatus.ToString() }
                        });
                }

                // Notify admin dashboard
                await _signalRService.SendToGroupAsync(
                    "admins",
                    "OrderUpdated",
                    new
                    {
                        orderId = orderUpdatedEvent.OrderId,
                        customerId = orderUpdatedEvent.CustomerId,
                        vendorId = orderUpdatedEvent.VendorId,
                        newStatus = orderUpdatedEvent.NewStatus.ToString()
                    });

                _logger.LogInformation(
                    "Successfully processed OrderUpdated event for order {OrderId}",
                    orderUpdatedEvent.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OrderUpdated event");
                throw;
            }
        }

        private bool ShouldSendPushNotification(OrderStatus status)
        {
            return status == OrderStatus.Confirmed ||
                   status == OrderStatus.PickedUp ||
                   status == OrderStatus.Delivered;
        }

        private string GetStatusMessage(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Confirmed => "Your order has been confirmed and is being prepared!",
                OrderStatus.PickedUp => "Your order has been picked up by the delivery person!",
                OrderStatus.Delivered => "Your order has been delivered. Enjoy your meal!",
                _ => $"Your order status has been updated to {status}"
            };
        }
    }
}