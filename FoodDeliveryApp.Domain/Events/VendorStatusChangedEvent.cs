using FoodDeliveryApp.Domain.Models;

namespace FoodDeliveryApp.Domain.Events
{
    public class VendorStatusChangedEvent : DomainEvent
    {
        public string VendorId { get; set; }
        public string VendorName { get; set; }
        public VendorStatus OldStatus { get; set; }
        public VendorStatus NewStatus { get; set; }
        public string? OrderId { get; set; }
        public OrderStatus? OrderStatus { get; set; }

        public VendorStatusChangedEvent() : base("VendorStatusChanged", "vendors")
        {
        }
    }
}