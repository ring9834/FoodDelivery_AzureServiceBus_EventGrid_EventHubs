POST https://api-fooddelivery-<random>.azurewebsites.net/api/orders
Content-Type: application/json

{
  "customerId": "customer-123",
  "vendorId": "vendor-456",
  "items": [
    {
      "itemId": "item-1",
      "itemName": "Burger",
      "quantity": 2,
      "price": 9.99
    }
  ],
  "deliveryAddress": {
    "street": "123 Main St",
    "city": "Seattle",
    "state": "WA",
    "zipCode": "98101",
    "latitude": 47.6062,
    "longitude": -122.3321
  }
}