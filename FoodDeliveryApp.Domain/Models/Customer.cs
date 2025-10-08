using System;
using Newtonsoft.Json;

namespace FoodDeliveryApp.Domain.Models
{
    public class Customer
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("defaultAddress")]
        public Address? DefaultAddress { get; set; }

        [JsonProperty("savedAddresses")]
        public List<Address> SavedAddresses { get; set; } = new();

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonProperty("partitionKey")]
        public string PartitionKey => Id;
    }
}