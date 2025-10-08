using System;

namespace FoodDeliveryApp.Domain.Events
{
    public abstract class DomainEvent
    {
        public string EventId { get; set; } = Guid.NewGuid().ToString();
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public string EventType { get; set; }
        public string Subject { get; set; }

        protected DomainEvent(string eventType, string subject)
        {
            EventType = eventType;
            Subject = subject;
        }
    }
}