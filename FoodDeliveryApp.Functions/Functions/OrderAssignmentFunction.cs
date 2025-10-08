using FoodDeliveryApp.Core.Interfaces;
using FoodDeliveryApp.Domain.Events;
using FoodDeliveryApp.Domain.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FoodDeliveryApp.Functions.Functions
{
    public class OrderAssignmentFunction
    {
        private readonly ICosmosDbService<Order> _orderService;
        private readonly ICosmosDbService<DeliveryPerson> _deliveryPersonService;
        private readonly ICosmosDbService<Vendor> _vendorService;
        private readonly IRedisCache _redisCache;
        private readonly IEventGridPublisher _eventGridPublisher;
        private readonly ILogger<OrderAssignmentFunction> _logger;

        public OrderAssignmentFunction(
            ICosmosDbService<Order> orderService,
            ICosmosDbService<DeliveryPerson> deliveryPersonService,
            ICosmosDbService<Vendor> vendorService,
            IRedisCache redisCache,
            IEventGridPublisher eventGridPublisher,
            ILogger<OrderAssignmentFunction> logger)
        {
            _orderService = orderService;
            _deliveryPersonService = deliveryPersonService;
            _vendorService = vendorService;
            _redisCache = redisCache;
            _eventGridPublisher = eventGridPublisher;
            _logger = logger;
        }

        [Function("OrderAssignmentFunction")]
        public async Task Run(
            [ServiceBusTrigger("order-assignment", Connection = "ServiceBusConnection")]
            string message)
        {
            try
            {
                _logger.LogInformation("Processing order assignment: {Message}", message);

                var assignmentRequest = JsonSerializer.Deserialize<OrderAssignmentRequestEvent>(message);

                // Get available delivery persons
                var query = "SELECT * FROM c WHERE c.status = 'Available' AND c.isActive = true";
                var availableDrivers = await _deliveryPersonService.GetItemsAsync(query);

                if (!availableDrivers.Any())
                {
                    _logger.LogWarning("No available delivery persons for order {OrderId}", assignmentRequest.OrderId);
                    return;
                }

                // Simple assignment logic: find closest available driver
                DeliveryPerson selectedDriver = null;
                double minDistance = double.MaxValue;

                foreach (var driver in availableDrivers)
                {
                    if (driver.CurrentLocation == null) continue;

                    var distance = CalculateDistance(
                        driver.CurrentLocation.Latitude,
                        driver.CurrentLocation.Longitude,
                        assignmentRequest.VendorLatitude,
                        assignmentRequest.VendorLongitude);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        selectedDriver = driver;
                    }
                }

                if (selectedDriver == null)
                {
                    _logger.LogWarning("No suitable delivery person found for order {OrderId}", assignmentRequest.OrderId);
                    return;
                }

                // Update order with assigned delivery person
                var order = await _orderService.GetItemAsync(assignmentRequest.OrderId, assignmentRequest.OrderId);
                if (order == null)
                {
                    _logger.LogError("Order {OrderId} not found", assignmentRequest.OrderId);
                    return;
                }

                order.DeliveryPersonId = selectedDriver.Id;
                order.Status = OrderStatus.Confirmed;
                order.EstimatedDeliveryTime = DateTime.UtcNow.AddMinutes(30 + (minDistance * 2));
                await _orderService.UpdateItemAsync(order.Id, order);

                // Update delivery person status
                selectedDriver.Status = DeliveryPersonStatus.Busy;
                selectedDriver.CurrentOrderId = order.Id;
                await _deliveryPersonService.UpdateItemAsync(selectedDriver.Id, selectedDriver);

                // Update cache
                await _redisCache.DeleteAsync($"delivery-person:available:{selectedDriver.Id}");

                // Get vendor info
                var vendor = await _vendorService.GetItemAsync(assignmentRequest.VendorId, assignmentRequest.VendorId);

                // Publish OrderDispatched event
                var dispatchedEvent = new OrderDispatchedEvent
                {
                    OrderId = order.Id,
                    CustomerId = order.CustomerId,
                    VendorId = order.VendorId,
                    DeliveryPersonId = selectedDriver.Id,
                    DeliveryPersonName = selectedDriver.Name,
                    PickupAddress = vendor.Address,
                    DeliveryAddress = order.DeliveryAddress,
                    EstimatedPickupTime = DateTime.UtcNow.AddMinutes(15),
                    EstimatedDeliveryTime = order.EstimatedDeliveryTime.Value
                };

                await _eventGridPublisher.PublishEventAsync(dispatchedEvent);

                _logger.LogInformation(
                    "Order {OrderId} assigned to delivery person {DeliveryPersonId}",
                    order.Id,
                    selectedDriver.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order assignment");
                throw;
            }
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Haversine formula to calculate distance in kilometers
            var R = 6371; // Radius of the earth in km
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double deg)
        {
            return deg * (Math.PI / 180);
        }

    }
}