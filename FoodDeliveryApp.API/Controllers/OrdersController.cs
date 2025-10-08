// File: Controllers/OrdersController.cs
using FoodDeliveryApp.Core.DTOs;
using FoodDeliveryApp.Core.Interfaces;
using FoodDeliveryApp.Domain.Events;
using FoodDeliveryApp.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading.Tasks;

namespace FoodDeliveryApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ICosmosDbService<Order> _orderService;
        private readonly ICosmosDbService<Vendor> _vendorService;
        private readonly ICosmosDbService<Customer> _customerService;
        private readonly IEventGridPublisher _eventGridPublisher;
        private readonly IServiceBusPublisher _serviceBusPublisher;
        private readonly ISignalRNotificationService _signalRService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            ICosmosDbService<Order> orderService,
            ICosmosDbService<Vendor> vendorService,
            ICosmosDbService<Customer> customerService,
            IEventGridPublisher eventGridPublisher,
            IServiceBusPublisher serviceBusPublisher,
            ISignalRNotificationService signalRService,
            ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _vendorService = vendorService;
            _customerService = customerService;
            _eventGridPublisher = eventGridPublisher;
            _serviceBusPublisher = serviceBusPublisher;
            _signalRService = signalRService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                // Validate customer and vendor
                var customer = await _customerService.GetItemAsync(
                    request.CustomerId,
                    request.CustomerId);

                if (customer == null)
                    return NotFound("Customer not found");

                var vendor = await _vendorService.GetItemAsync(
                    request.VendorId,
                    request.VendorId);

                if (vendor == null)
                    return NotFound("Vendor not found");

                if (vendor.Status == VendorStatus.Offline)
                    return BadRequest("Vendor is currently offline");

                // Create order
                var order = new Order
                {
                    CustomerId = request.CustomerId,
                    VendorId = request.VendorId,
                    Items = request.Items.Select(item => new OrderItem
                    {
                        ItemId = item.ItemId,
                        ItemName = item.ItemName,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        SpecialInstructions = item.SpecialInstructions
                    }).ToList(),
                    TotalAmount = request.Items.Sum(i => i.Price * i.Quantity),
                    DeliveryAddress = new Address
                    {
                        Street = request.DeliveryAddress.Street,
                        City = request.DeliveryAddress.City,
                        State = request.DeliveryAddress.State,
                        ZipCode = request.DeliveryAddress.ZipCode,
                        Latitude = request.DeliveryAddress.Latitude,
                        Longitude = request.DeliveryAddress.Longitude
                    },
                    Status = OrderStatus.Pending
                };

                order.StatusHistory.Add(new StatusHistory
                {
                    Status = OrderStatus.Pending,
                    Timestamp = DateTime.UtcNow,
                    UpdatedBy = "System",
                    Notes = "Order created"
                });

                // Save to Cosmos DB
                var createdOrder = await _orderService.CreateItemAsync(order);

                // Publish OrderCreated event to Event Grid
                var orderCreatedEvent = new OrderCreatedEvent
                {
                    OrderId = createdOrder.Id,
                    CustomerId = createdOrder.CustomerId,
                    VendorId = createdOrder.VendorId,
                    TotalAmount = createdOrder.TotalAmount,
                    DeliveryAddress = createdOrder.DeliveryAddress,
                    ItemCount = createdOrder.Items.Count
                };

                await _eventGridPublisher.PublishEventAsync(orderCreatedEvent);

                // Send assignment request to Service Bus
                var assignmentRequest = new OrderAssignmentRequestEvent
                {
                    OrderId = createdOrder.Id,
                    VendorId = createdOrder.VendorId,
                    VendorLatitude = vendor.Address.Latitude,
                    VendorLongitude = vendor.Address.Longitude,
                    DeliveryLatitude = createdOrder.DeliveryAddress.Latitude,
                    DeliveryLongitude = createdOrder.DeliveryAddress.Longitude
                };

                await _serviceBusPublisher.SendMessageAsync(
                    "order-assignment",
                    assignmentRequest);

                // Send real-time notification to vendor
                await _signalRService.SendToUserAsync(
                    vendor.Id,
                    "NewOrder",
                    new { order = createdOrder });

                _logger.LogInformation(
                    "Order {OrderId} created successfully",
                    createdOrder.Id);

                return CreatedAtAction(
                    nameof(GetOrder),
                    new { id = createdOrder.Id },
                    createdOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, "An error occurred while creating the order");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(string id, [FromQuery] string customerId)
        {
            try
            {
                var order = await _orderService.GetItemAsync(id, customerId);

                if (order == null)
                    return NotFound();

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order {OrderId}", id);
                return StatusCode(500, "An error occurred while retrieving the order");
            }
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetCustomerOrders(string customerId)
        {
            try
            {
                var query = $"SELECT * FROM c WHERE c.customerId = '{customerId}' ORDER BY c.createdAt DESC";
                var orders = await _orderService.GetItemsAsync(query);

                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for customer {CustomerId}", customerId);
                return StatusCode(500, "An error occurred while retrieving orders");
            }
        }

        [HttpGet("vendor/{vendorId}")]
        public async Task<IActionResult> GetVendorOrders(string vendorId)
        {
            try
            {
                var query = $"SELECT * FROM c WHERE c.vendorId = '{vendorId}' ORDER BY c.createdAt DESC";
                var orders = await _orderService.GetItemsAsync(query);

                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for vendor {VendorId}", vendorId);
                return StatusCode(500, "An error occurred while retrieving orders");
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(
            string id,
            [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                var order = await _orderService.GetItemAsync(id, request.OrderId);

                if (order == null)
                    return NotFound();

                var oldStatus = order.Status;
                order.Status = request.NewStatus;
                order.UpdatedAt = DateTime.UtcNow;

                order.StatusHistory.Add(new StatusHistory
                {
                    Status = request.NewStatus,
                    Timestamp = DateTime.UtcNow,
                    UpdatedBy = request.UpdatedBy,
                    Notes = request.Notes
                });

                // Update in Cosmos DB
                await _orderService.UpdateItemAsync(id, order);

                // Publish OrderUpdated event
                var orderUpdatedEvent = new OrderUpdatedEvent
                {
                    OrderId = order.Id,
                    CustomerId = order.CustomerId,
                    VendorId = order.VendorId,
                    OldStatus = oldStatus,
                    NewStatus = request.NewStatus,
                    UpdatedBy = request.UpdatedBy
                };

                await _eventGridPublisher.PublishEventAsync(orderUpdatedEvent);

                // Send real-time notification to customer
                await _signalRService.SendToUserAsync(
                    order.CustomerId,
                    "OrderStatusUpdated",
                    new { orderId = order.Id, status = request.NewStatus });

                _logger.LogInformation(
                    "Order {OrderId} status updated from {OldStatus} to {NewStatus}",
                    order.Id,
                    oldStatus,
                    request.NewStatus);

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for {OrderId}", id);
                return StatusCode(500, "An error occurred while updating order status");
            }
        }
    }
}