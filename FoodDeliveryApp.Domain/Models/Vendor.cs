using System;
using Newtonsoft.Json;

namespace FoodDeliveryApp.Domain.Models
{
    public class Vendor
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("address")]
        public Address Address { get; set; }

        [JsonProperty("status")]
        public VendorStatus Status { get; set; } = VendorStatus.Online;

        [JsonProperty("preparationTime")]
        public int AveragePreparationTimeMinutes { get; set; } = 30;

        [JsonProperty("rating")]
        public double Rating { get; set; }

        [JsonProperty("isActive")]
        public bool IsActive { get; set; } = true;

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonProperty("partitionKey")]
        public string PartitionKey => Id;
    }

    public enum VendorStatus
    {
        Online,
        Busy,
        Offline
    }
}