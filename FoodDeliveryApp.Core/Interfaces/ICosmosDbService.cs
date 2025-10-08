using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoodDeliveryApp.Core.Interfaces
{
    public interface ICosmosDbService<T> where T : class
    {
        Task<T> GetItemAsync(string id, string partitionKey);
        Task<IEnumerable<T>> GetItemsAsync(string queryString);
        Task<T> CreateItemAsync(T item);
        Task<T> UpdateItemAsync(string id, T item);
        Task DeleteItemAsync(string id, string partitionKey);
    }
}