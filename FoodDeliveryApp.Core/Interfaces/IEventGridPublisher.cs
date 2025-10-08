using FoodDeliveryApp.Domain.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoodDeliveryApp.Core.Interfaces
{
    public interface IEventGridPublisher
    {
        Task PublishEventAsync<T>(T domainEvent) where T : DomainEvent;
        Task PublishEventsAsync<T>(IEnumerable<T> domainEvents) where T : DomainEvent;
    }
}