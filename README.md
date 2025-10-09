# FoodDelivery via Azure Service Bus, Event Grid, Event Hubs
This is an bac-kend and Cloud services implementation with .Net9, C#, RESTful API alongside Azure Service Bus, Event Grid, Event Hubs, Azure Functions, Azure SingleR, Azure Notification Hub, and Azure Redis.

## Main Functions
:bulb: Customers can see their order status and the real-time location of delivery driver.

:bulb: Vendors can know new orders coming and get hints; 

:bulb: Delivery driver can be notified they are dispatched new orders automatically; 

:bulb: Administrators can overlook all or part of delivery men of their real-time GPS location and delivery status as well as watching the real-time status of all or part of the vendors about their preparing foods for packadges.

## Logic Flow
:sparkles: Customer places order → API writes to Cosmos DB → Publishes OrderCreated event;

:sparkles: Event Grid → Triggers OrderCreatedFunction → Sends to Service Bus for assignment;

:sparkles: Service Bus → OrderAssignmentFunction finds nearest driver → Assigns order;

:sparkles: Assignment → Updates Cosmos DB → Publishes OrderDispatched event;

:sparkles: OrderDispatched → Notifies customer & driver via SignalR + Push Notifications;

:sparkles: Driver location updates → Sent to Event Hubs → Processed and pushed via SignalR;

:sparkles: Vendor status changes → Cosmos DB + Event Grid → Real-time dashboard updates.
