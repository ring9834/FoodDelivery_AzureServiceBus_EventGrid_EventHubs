using System;
using System.Threading.Tasks;

namespace FoodDeliveryApp.Core.Interfaces
{
    public interface IRedisCache
    {
        Task<T?> GetAsync<T>(string key) where T : class;
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
        Task<bool> DeleteAsync(string key);
        Task<bool> ExistsAsync(string key);
        Task<long> IncrementAsync(string key);
        Task<long> DecrementAsync(string key);
    }
}