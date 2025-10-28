# FoodDelivery via Azure Service Bus, Event Grid, Event Hubs
This is an back-end and Cloud services implementation with .Net9, C#, RESTful API alongside Azure Service Bus, Event Grid, Event Hubs, Azure Functions, Azure SingleR, Azure Notification Hub, and Azure Redis.

## Features
:bulb: Customers can see their order status and the real-time location of delivery driver.

:bulb: Vendors can know new orders coming and get hints; 

:bulb: Delivery driver can be notified they are dispatched new orders automatically; 

:bulb: Administrators can monitor all or select delivery drivers' real-time GPS locations and delivery statuses, as well as track the real-time progress of all or specific vendors preparing food for packaging.

## Logic Flow
:sparkles: Customer places order → API writes to Cosmos DB → Publishes OrderCreated event;

:sparkles: Event Grid → Triggers OrderCreatedFunction → Sends to Service Bus for assignment;

:sparkles: Service Bus → OrderAssignmentFunction finds nearest driver → Assigns order;

:sparkles: Assignment → Updates Cosmos DB → Publishes OrderDispatched event;

:sparkles: OrderDispatched → Notifies customer & driver via SignalR + Push Notifications;

:sparkles: Driver location updates → Sent to Event Hubs → Processed and pushed via SignalR;

:sparkles: Vendor status changes → Cosmos DB + Event Grid → Real-time dashboard updates.
