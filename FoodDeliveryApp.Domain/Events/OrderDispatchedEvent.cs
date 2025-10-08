using FoodDeliveryApp.Domain.Models;
using System;

namespace FoodDeliveryApp.Domain.Events
{
    public class OrderDispatchedEvent : DomainEvent
    {
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public string VendorId { get; set; }
        public string DeliveryPersonId { get; set; }
        public string DeliveryPersonName { get; set; }
        public Address PickupAddress { get; set; }
        public Address DeliveryAddress { get; set; }
        public DateTime EstimatedPickupTime { get; set; }
        public DateTime EstimatedDeliveryTime { get; set; }

        public OrderDispatchedEvent() : base("OrderDispatched", "orders")
        {
        }
    }
}