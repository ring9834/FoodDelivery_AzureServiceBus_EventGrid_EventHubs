# FoodDelivery via Azure Service Bus, Event Grid, Event Hubs
This is an bac-kend and Cloud services implementation with .Net9, C#, RESTful API alongside Azure Service Bus, Event Grid, Event Hubs, Azure Functions, Azure SingleR, Azure Notification Hub, Azure Redis.

## Requirements
Customers can see their order status and the real-time location of delivery driver.

Vendors can know new orders coming and get hints; 

Delivery driver can be notified they are dispatched new orders automatically; 

Administrators can overlook all or part of delivery men of their real-time GPS location and delivery status as well as watching the real-time status of all or part of the vendors about their preparing foods for packadges; 

## Logic Flow
Customer places order → API writes to Cosmos DB → Publishes OrderCreated event;

Event Grid → Triggers OrderCreatedFunction → Sends to Service Bus for assignment;

Service Bus → OrderAssignmentFunction finds nearest driver → Assigns order;

Assignment → Updates Cosmos DB → Publishes OrderDispatched event;

OrderDispatched → Notifies customer & driver via SignalR + Push Notifications;

Driver location updates → Sent to Event Hubs → Processed and pushed via SignalR;

Vendor status changes → Cosmos DB + Event Grid → Real-time dashboard updates.
