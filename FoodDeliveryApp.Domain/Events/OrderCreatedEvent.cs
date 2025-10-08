using FoodDeliveryApp.Domain.Models;

namespace FoodDeliveryApp.Domain.Events
{
    public class OrderCreatedEvent : DomainEvent
    {
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public string VendorId { get; set; }
        public decimal TotalAmount { get; set; }
        public Address DeliveryAddress { get; set; }
        public int ItemCount { get; set; }

        public OrderCreatedEvent() : base("OrderCreated", "orders")
        {
        }
    }
}