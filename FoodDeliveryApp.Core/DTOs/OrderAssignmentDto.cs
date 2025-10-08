namespace FoodDeliveryApp.Core.DTOs
{
    public class OrderAssignmentDto
    {
        public string OrderId { get; set; }
        public string DeliveryPersonId { get; set; }
        public DateTime EstimatedPickupTime { get; set; }
        public DateTime EstimatedDeliveryTime { get; set; }
        public double DistanceKm { get; set; }
    }
}