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
    public class VendorsController : ControllerBase
    {
        private readonly ICosmosDbService<Vendor> _vendorService;
        private readonly IEventGridPublisher _eventGridPublisher;
        private readonly ISignalRNotificationService _signalRService;
        private readonly ILogger<VendorsController> _logger;

        public VendorsController(
            ICosmosDbService<Vendor> vendorService,
            IEventGridPublisher eventGridPublisher,
            ISignalRNotificationService signalRService,
            ILogger<VendorsController> logger)
        {
            _vendorService = vendorService;
            _eventGridPublisher = eventGridPublisher;
            _signalRService = signalRService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVendor(string id)
        {
            try
            {
                var vendor = await _vendorService.GetItemAsync(id, id);

                if (vendor == null)
                    return NotFound();

                return Ok(vendor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vendor {VendorId}", id);
                return StatusCode(500, "An error occurred while retrieving vendor");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllVendors()
        {
            try
            {
                var query = "SELECT * FROM c WHERE c.isActive = true";
                var vendors = await _vendorService.GetItemsAsync(query);

                return Ok(vendors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vendors");
                return StatusCode(500, "An error occurred while retrieving vendors");
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateVendorStatus(
            string id,
            [FromBody] UpdateVendorStatusRequest request)
        {
            try
            {
                var vendor = await _vendorService.GetItemAsync(id, id);

                if (vendor == null)
                    return NotFound();

                var oldStatus = vendor.Status;
                vendor.Status = request.Status;

                await _vendorService.UpdateItemAsync(id, vendor);

                // Publish VendorStatusChanged event
                var statusChangedEvent = new VendorStatusChangedEvent
                {
                    VendorId = vendor.Id,
                    VendorName = vendor.Name,
                    OldStatus = oldStatus,
                    NewStatus = request.Status,
                    OrderId = request.OrderId,
                    OrderStatus = request.OrderStatus
                };

                await _eventGridPublisher.PublishEventAsync(statusChangedEvent);

                // Notify admin dashboards
                await _signalRService.SendToGroupAsync(
                    "admins",
                    "VendorStatusChanged",
                    new
                    {
                        vendorId = vendor.Id,
                        vendorName = vendor.Name,
                        status = request.Status
                    });

                _logger.LogInformation(
                    "Vendor {VendorId} status updated from {OldStatus} to {NewStatus}",
                    id,
                    oldStatus,
                    request.Status);

                return Ok(vendor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vendor status {VendorId}", id);
                return StatusCode(500, "An error occurred while updating vendor status");
            }
        }
    }

    public class UpdateVendorStatusRequest
    {
        public VendorStatus Status { get; set; }
        public string? OrderId { get; set; }
        public OrderStatus? OrderStatus { get; set; }
    }
}