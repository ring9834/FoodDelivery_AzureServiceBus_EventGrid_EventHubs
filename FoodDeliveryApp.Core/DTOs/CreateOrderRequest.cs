using FoodDeliveryApp.Domain.Models;
using System.Collections.Generic;

namespace FoodDeliveryApp.Core.DTOs
{
    public class CreateOrderRequest
    {
        public string CustomerId { get; set; }
        public string VendorId { get; set; }
        public List<OrderItemDto> Items { get; set; }
        public AddressDto DeliveryAddress { get; set; }
    }

    public class OrderItemDto
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? SpecialInstructions { get; set; }
    }

    public class AddressDto
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}