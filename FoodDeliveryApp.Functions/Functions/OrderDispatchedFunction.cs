using Azure.Messaging.EventGrid;
using FoodDeliveryApp.Core.Interfaces;
using FoodDeliveryApp.Domain.Events;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace FoodDeliveryApp.Functions.Functions
{
    public class OrderDispatchedFunction
    {
        private readonly ISignalRNotificationService _signalRService;
        private readonly INotificationHubService _notificationHubService;
        private readonly ILogger<OrderDispatchedFunction> _logger;

        public OrderDispatchedFunction(
            ISignalRNotificationService signalRService,
            INotificationHubService notificationHubService,
            ILogger<OrderDispatchedFunction> logger)
        {
            _signalRService = signalRService;
            _notificationHubService = notificationHubService;
            _logger = logger;
        }

        [Function("OrderDispatchedFunction")]
        public async Task Run(
            [EventGridTrigger] EventGridEvent eventGridEvent)
        {
            try
            {
                _logger.LogInformation("Processing OrderDispatched event: {EventId}", eventGridEvent.Id);

                var orderDispatchedEvent = JsonSerializer.Deserialize<OrderDispatchedEvent>(
                    eventGridEvent.Data.ToString());

                // Send real-time notification to customer
                await _signalRService.SendToUserAsync(
                    orderDispatchedEvent.CustomerId,
                    "OrderDispatched",
                    new
                    {
                        orderId = orderDispatchedEvent.OrderId,
                        deliveryPersonId = orderDispatchedEvent.DeliveryPersonId,
                        deliveryPersonName = orderDispatchedEvent.DeliveryPersonName,
                        estimatedDeliveryTime = orderDispatchedEvent.EstimatedDeliveryTime
                    });

                // Send push notification to customer
                await _notificationHubService.SendPushNotificationAsync(
                    orderDispatchedEvent.CustomerId,
                    "Delivery Assigned",
                    $"{orderDispatchedEvent.DeliveryPersonName} will deliver your order!",
                    new Dictionary<string, string>
                    {
                        { "orderId", orderDispatchedEvent.OrderId },
                        { "deliveryPersonId", orderDispatchedEvent.DeliveryPersonId }
                    });

                // Send notification to delivery person
                await _signalRService.SendToUserAsync(
                    orderDispatchedEvent.DeliveryPersonId,
                    "NewDeliveryAssignment",
                    new
                    {
                        orderId = orderDispatchedEvent.OrderId,
                        pickupAddress = orderDispatchedEvent.PickupAddress,
                        deliveryAddress = orderDispatchedEvent.DeliveryAddress,
                        estimatedPickupTime = orderDispatchedEvent.EstimatedPickupTime
                    });

                // Send push notification to delivery person
                await _notificationHubService.SendPushNotificationAsync(
                    orderDispatchedEvent.DeliveryPersonId,
                    "New Delivery",
                    "You have been assigned a new delivery!",
                    new Dictionary<string, string>
                    {
                        { "orderId", orderDispatchedEvent.OrderId }
                    });

                _logger.LogInformation(
                    "Successfully processed OrderDispatched event for order {OrderId}",
                    orderDispatchedEvent.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OrderDispatched event");
                throw;
            }
        }
    }
}