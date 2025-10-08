namespace FoodDeliveryApp.Domain.Events
{
    public class OrderAssignmentRequestEvent : DomainEvent
    {
        public string OrderId { get; set; }
        public string VendorId { get; set; }
        public double VendorLatitude { get; set; }
        public double VendorLongitude { get; set; }
        public double DeliveryLatitude { get; set; }
        public double DeliveryLongitude { get; set; }
        public int Priority { get; set; } = 1;

        public OrderAssignmentRequestEvent() : base("OrderAssignmentRequest", "assignments")
        {
        }
    }
}