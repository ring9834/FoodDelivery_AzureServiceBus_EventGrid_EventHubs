using Azure.Messaging.EventGrid;
using FoodDeliveryApp.Core.Interfaces;
using FoodDeliveryApp.Domain.Events;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;

namespace FoodDeliveryApp.Functions.Functions
{
    public class OrderCreatedFunction
    {
        private readonly IServiceBusPublisher _serviceBusPublisher;
        private readonly ISignalRNotificationService _signalRService;
        private readonly ILogger<OrderCreatedFunction> _logger;

        public OrderCreatedFunction(
            IServiceBusPublisher serviceBusPublisher,
            ISignalRNotificationService signalRService,
            ILogger<OrderCreatedFunction> logger)
        {
            _serviceBusPublisher = serviceBusPublisher;
            _signalRService = signalRService;
            _logger = logger;
        }

        [Function("OrderCreatedFunction")]
        public async Task Run(
            [EventGridTrigger] EventGridEvent eventGridEvent)
        {
            try
            {
                _logger.LogInformation("Processing OrderCreated event: {EventId}", eventGridEvent.Id);

                var orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(
                    eventGridEvent.Data.ToString());

                // Create assignment request
                var assignmentRequest = new OrderAssignmentRequestEvent
                {
                    OrderId = orderCreatedEvent.OrderId,
                    VendorId = orderCreatedEvent.VendorId,
                    VendorLatitude = orderCreatedEvent.DeliveryAddress.Latitude,
                    VendorLongitude = orderCreatedEvent.DeliveryAddress.Longitude,
                    DeliveryLatitude = orderCreatedEvent.DeliveryAddress.Latitude,
                    DeliveryLongitude = orderCreatedEvent.DeliveryAddress.Longitude
                };

                // Send to Service Bus for assignment processing
                await _serviceBusPublisher.SendMessageAsync(
                    "order-assignment",
                    assignmentRequest);

                // Send real-time notification to vendor
                await _signalRService.SendToUserAsync(
                    orderCreatedEvent.VendorId,
                    "NewOrder",
                    new
                    {
                        orderId = orderCreatedEvent.OrderId,
                        customerId = orderCreatedEvent.CustomerId,
                        totalAmount = orderCreatedEvent.TotalAmount,
                        itemCount = orderCreatedEvent.ItemCount
                    });

                _logger.LogInformation(
                    "Successfully processed OrderCreated event for order {OrderId}",
                    orderCreatedEvent.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OrderCreated event");
                throw;
            }
        }
    }
}