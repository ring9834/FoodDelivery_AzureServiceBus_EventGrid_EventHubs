using FoodDeliveryApp.Domain.Models;

namespace FoodDeliveryApp.Core.DTOs
{
    public class UpdateOrderStatusRequest
    {
        public string OrderId { get; set; }
        public OrderStatus NewStatus { get; set; }
        public string UpdatedBy { get; set; }
        public string? Notes { get; set; }
    }
}