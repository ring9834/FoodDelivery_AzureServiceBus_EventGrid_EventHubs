using FoodDeliveryApp.Core.DTOs;
using FoodDeliveryApp.Core.Interfaces;
using FoodDeliveryApp.Domain.Events;
using FoodDeliveryApp.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace FoodDeliveryApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveryController : ControllerBase
    {
        private readonly ICosmosDbService<DeliveryPerson> _deliveryPersonService;
        private readonly IEventHubPublisher _eventHubPublisher;
        private readonly IRedisCache _redisCache;
        private readonly ISignalRNotificationService _signalRService;
        private readonly ILogger<DeliveryController> _logger;

        public DeliveryController(
            ICosmosDbService<DeliveryPerson> deliveryPersonService,
            IEventHubPublisher eventHubPublisher,
            IRedisCache redisCache,
            ISignalRNotificationService signalRService,
            ILogger<DeliveryController> logger)
        {
            _deliveryPersonService = deliveryPersonService;
            _eventHubPublisher = eventHubPublisher;
            _redisCache = redisCache;
            _signalRService = signalRService;
            _logger = logger;
        }

        [HttpPost("{deliveryPersonId}/location")]
        public async Task<IActionResult> UpdateLocation(
            string deliveryPersonId,
            [FromBody] LocationUpdateDto locationUpdate)
        {
            try
            {
                // Validate delivery person
                var deliveryPerson = await _deliveryPersonService.GetItemAsync(
                    deliveryPersonId,
                    deliveryPersonId);

                if (deliveryPerson == null)
                    return NotFound("Delivery person not found");

                // Create location object
                var location = new Location
                {
                    Latitude = locationUpdate.Latitude,
                    Longitude = locationUpdate.Longitude,
                    Timestamp = locationUpdate.Timestamp,
                    Accuracy = locationUpdate.Accuracy,
                    Speed = locationUpdate.Speed
                };

                // Update current location in Cosmos DB (throttled)
                deliveryPerson.CurrentLocation = location;
                await _deliveryPersonService.UpdateItemAsync(deliveryPersonId, deliveryPerson);

                // Store in Redis for fast access (5 minute expiration)
                var cacheKey = $"location:{deliveryPersonId}";
                await _redisCache.SetAsync(cacheKey, location, TimeSpan.FromMinutes(5));

                // Send to Event Hub for high-throughput telemetry
                var locationEvent = new DeliveryPersonLocationUpdatedEvent
                {
                    DeliveryPersonId = deliveryPersonId,
                    OrderId = deliveryPerson.CurrentOrderId,
                    Location = location,
                    Status = deliveryPerson.Status
                };

                await _eventHubPublisher.SendEventAsync(locationEvent);

                // If delivery person has an active order, notify customer via SignalR
                if (!string.IsNullOrEmpty(deliveryPerson.CurrentOrderId))
                {
                    // Note: We would need to fetch the order to get customer ID
                    // For now, sending to a group based on order ID
                    await _signalRService.SendToGroupAsync(
                        $"order-{deliveryPerson.CurrentOrderId}",
                        "DeliveryLocationUpdated",
                        new
                        {
                            orderId = deliveryPerson.CurrentOrderId,
                            deliveryPersonId = deliveryPersonId,
                            location = new
                            {
                                latitude = location.Latitude,
                                longitude = location.Longitude,
                                timestamp = location.Timestamp
                            }
                        });
                }

                _logger.LogInformation(
                    "Location updated for delivery person {DeliveryPersonId}",
                    deliveryPersonId);

                return Ok(new { message = "Location updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error updating location for delivery person {DeliveryPersonId}",
                    deliveryPersonId);
                return StatusCode(500, "An error occurred while updating location");
            }
        }

        [HttpGet("{deliveryPersonId}/location")]
        public async Task<IActionResult> GetCurrentLocation(string deliveryPersonId)
        {
            try
            {
                // Try Redis first for fast access
                var cacheKey = $"location:{deliveryPersonId}";
                var location = await _redisCache.GetAsync<Location>(cacheKey);

                if (location != null)
                {
                    return Ok(location);
                }

                // Fallback to Cosmos DB
                var deliveryPerson = await _deliveryPersonService.GetItemAsync(
                    deliveryPersonId,
                    deliveryPersonId);

                if (deliveryPerson == null)
                    return NotFound("Delivery person not found");

                return Ok(deliveryPerson.CurrentLocation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error retrieving location for delivery person {DeliveryPersonId}",
                    deliveryPersonId);
                return StatusCode(500, "An error occurred while retrieving location");
            }
        }

        [HttpGet("{deliveryPersonId}")]
        public async Task<IActionResult> GetDeliveryPerson(string deliveryPersonId)
        {
            try
            {
                var deliveryPerson = await _deliveryPersonService.GetItemAsync(
                    deliveryPersonId,
                    deliveryPersonId);

                if (deliveryPerson == null)
                    return NotFound();

                return Ok(deliveryPerson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error retrieving delivery person {DeliveryPersonId}",
                    deliveryPersonId);
                return StatusCode(500, "An error occurred while retrieving delivery person");
            }
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableDeliveryPersons()
        {
            try
            {
                var query = "SELECT * FROM c WHERE c.status = 'Available' AND c.isActive = true";
                var deliveryPersons = await _deliveryPersonService.GetItemsAsync(query);

                return Ok(deliveryPersons);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available delivery persons");
                return StatusCode(500, "An error occurred while retrieving delivery persons");
            }
        }

        [HttpPut("{deliveryPersonId}/status")]
        public async Task<IActionResult> UpdateDeliveryPersonStatus(
            string deliveryPersonId,
            [FromBody] UpdateDeliveryPersonStatusRequest request)
        {
            try
            {
                var deliveryPerson = await _deliveryPersonService.GetItemAsync(
                    deliveryPersonId,
                    deliveryPersonId);

                if (deliveryPerson == null)
                    return NotFound();

                var oldStatus = deliveryPerson.Status;
                deliveryPerson.Status = request.Status;

                await _deliveryPersonService.UpdateItemAsync(deliveryPersonId, deliveryPerson);

                // Update availability in Redis
                var availabilityKey = $"delivery-person:available:{deliveryPersonId}";
                if (request.Status == DeliveryPersonStatus.Available)
                {
                    await _redisCache.SetAsync(availabilityKey, "true", TimeSpan.FromHours(1));
                }
                else
                {
                    await _redisCache.DeleteAsync(availabilityKey);
                }

                _logger.LogInformation(
                    "Delivery person {DeliveryPersonId} status updated from {OldStatus} to {NewStatus}",
                    deliveryPersonId,
                    oldStatus,
                    request.Status);

                return Ok(deliveryPerson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error updating status for delivery person {DeliveryPersonId}",
                    deliveryPersonId);
                return StatusCode(500, "An error occurred while updating status");
            }
        }
    }

    public class UpdateDeliveryPersonStatusRequest
    {
        public DeliveryPersonStatus Status { get; set; }
    }
}