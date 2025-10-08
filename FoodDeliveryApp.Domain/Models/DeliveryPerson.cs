using System;
using Newtonsoft.Json;

namespace FoodDeliveryApp.Domain.Models
{
    public class DeliveryPerson
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("vehicleType")]
        public VehicleType VehicleType { get; set; }

        [JsonProperty("vehicleNumber")]
        public string VehicleNumber { get; set; }

        [JsonProperty("status")]
        public DeliveryPersonStatus Status { get; set; } = DeliveryPersonStatus.Offline;

        [JsonProperty("currentLocation")]
        public Location? CurrentLocation { get; set; }

        [JsonProperty("currentOrderId")]
        public string? CurrentOrderId { get; set; }

        [JsonProperty("rating")]
        public double Rating { get; set; }

        [JsonProperty("isActive")]
        public bool IsActive { get; set; } = true;

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonProperty("partitionKey")]
        public string PartitionKey => Id;
    }

    public class Location
    {
        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [JsonProperty("accuracy")]
        public double Accuracy { get; set; }

        [JsonProperty("speed")]
        public double? Speed { get; set; }
    }

    public enum VehicleType
    {
        Bike,
        Scooter,
        Car,
        Van
    }

    public enum DeliveryPersonStatus
    {
        Offline,
        Available,
        Busy,
        OnBreak
    }
}