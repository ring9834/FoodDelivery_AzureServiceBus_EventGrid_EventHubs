using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoodDeliveryApp.Core.Interfaces
{
    public interface IEventHubPublisher
    {
        Task SendEventAsync<T>(T eventData);
        Task SendEventsAsync<T>(IEnumerable<T> eventData);
    }
}