using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace FoodDeliveryApp.Domain.Models
{
    public class Order
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("customerId")]
        public string CustomerId { get; set; }

        [JsonProperty("vendorId")]
        public string VendorId { get; set; }

        [JsonProperty("deliveryPersonId")]
        public string? DeliveryPersonId { get; set; }

        [JsonProperty("items")]
        public List<OrderItem> Items { get; set; } = new();

        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("status")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [JsonProperty("deliveryAddress")]
        public Address DeliveryAddress { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [JsonProperty("estimatedDeliveryTime")]
        public DateTime? EstimatedDeliveryTime { get; set; }

        [JsonProperty("statusHistory")]
        public List<StatusHistory> StatusHistory { get; set; } = new();

        [JsonProperty("partitionKey")]
        public string PartitionKey => CustomerId;
    }

    public class OrderItem
    {
        [JsonProperty("itemId")]
        public string ItemId { get; set; }

        [JsonProperty("itemName")]
        public string ItemName { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("specialInstructions")]
        public string? SpecialInstructions { get; set; }
    }

    public class Address
    {
        [JsonProperty("street")]
        public string Street { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("zipCode")]
        public string ZipCode { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }
    }

    public class StatusHistory
    {
        [JsonProperty("status")]
        public OrderStatus Status { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("updatedBy")]
        public string UpdatedBy { get; set; }

        [JsonProperty("notes")]
        public string? Notes { get; set; }
    }

    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Preparing,
        ReadyForPickup,
        PickedUp,
        InTransit,
        Delivered,
        Cancelled
    }
}