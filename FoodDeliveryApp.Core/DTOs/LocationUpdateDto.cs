using System;

namespace FoodDeliveryApp.Core.DTOs
{
    public class LocationUpdateDto
    {
        public string DeliveryPersonId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Accuracy { get; set; }
        public double? Speed { get; set; }
        public DateTime Timestamp { get; set; }
    }
}