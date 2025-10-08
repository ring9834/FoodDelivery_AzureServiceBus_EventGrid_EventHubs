POST https://api-fooddelivery-<random>.azurewebsites.net/api/delivery/{deliveryPersonId}/location
Content-Type: application/json

{
  "deliveryPersonId": "driver-789",
  "latitude": 47.6062,
  "longitude": -122.3321,
  "accuracy": 10.0,
  "speed": 25.5,
  "timestamp": "2025-10-08T12:00:00Z"
}