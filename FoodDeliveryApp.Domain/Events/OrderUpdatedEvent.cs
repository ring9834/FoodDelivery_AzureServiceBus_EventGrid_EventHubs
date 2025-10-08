using FoodDeliveryApp.Domain.Models;

namespace FoodDeliveryApp.Domain.Events
{
    public class OrderUpdatedEvent : DomainEvent
    {
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public string VendorId { get; set; }
        public OrderStatus OldStatus { get; set; }
        public OrderStatus NewStatus { get; set; }
        public string UpdatedBy { get; set; }

        public OrderUpdatedEvent() : base("OrderUpdated", "orders")
        {
        }
    }
}