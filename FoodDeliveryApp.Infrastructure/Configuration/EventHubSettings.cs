namespace FoodDeliveryApp.Infrastructure.Configuration
{
    public class EventHubSettings
    {
        public string ConnectionString { get; set; }
        public string EventHubName { get; set; } = "location-telemetry";
    }
}