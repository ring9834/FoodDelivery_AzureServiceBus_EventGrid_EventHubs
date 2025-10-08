using FoodDeliveryApp.Domain.Models;

namespace FoodDeliveryApp.Domain.Events
{
    public class DeliveryPersonLocationUpdatedEvent : DomainEvent
    {
        public string DeliveryPersonId { get; set; }
        public string? OrderId { get; set; }
        public Location Location { get; set; }
        public DeliveryPersonStatus Status { get; set; }

        public DeliveryPersonLocationUpdatedEvent() : base("DeliveryPersonLocationUpdated", "delivery")
        {
        }
    }
}