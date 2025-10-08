namespace FoodDeliveryApp.Infrastructure.Configuration
{
    public class ServiceBusSettings
    {
        public string ConnectionString { get; set; }
        public string OrderAssignmentTopic { get; set; } = "order-assignment";
        public string NotificationQueue { get; set; } = "notifications";
    }
}