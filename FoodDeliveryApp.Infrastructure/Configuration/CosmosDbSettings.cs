namespace FoodDeliveryApp.Infrastructure.Configuration
{
    public class CosmosDbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string OrdersContainer { get; set; } = "Orders";
        public string VendorsContainer { get; set; } = "Vendors";
        public string DeliveryPersonsContainer { get; set; } = "DeliveryPersons";
        public string CustomersContainer { get; set; } = "Customers";
    }
}