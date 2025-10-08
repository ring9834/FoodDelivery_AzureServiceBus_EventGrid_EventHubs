using System.Threading.Tasks;

namespace FoodDeliveryApp.Core.Interfaces
{
    public interface IServiceBusPublisher
    {
        Task SendMessageAsync<T>(string queueOrTopicName, T message);
        Task SendMessageAsync<T>(string queueOrTopicName, T message, string sessionId);
        Task SendMessagesAsync<T>(string queueOrTopicName, IEnumerable<T> messages);
    }
}