using FoodDeliveryApp.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace FoodDeliveryApp.Infrastructure.Services
{
    public class RedisCacheService : IRedisCache
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(
            IConfiguration configuration,
            ILogger<RedisCacheService> logger)
        {
            var connectionString = configuration["Redis:ConnectionString"];
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _database = _redis.GetDatabase();
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var value = await _database.StringGetAsync(key);

                if (value.IsNullOrEmpty)
                    return null;

                return JsonSerializer.Deserialize<T>(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value from Redis for key {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var json = JsonSerializer.Serialize(value);
                await _database.StringSetAsync(key, json, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value in Redis for key {Key}", key);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string key)
        {
            try
            {
                return await _database.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting key from Redis {Key}", key);
                return false;
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                return await _database.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking key existence in Redis {Key}", key);
                return false;
            }
        }

        public async Task<long> IncrementAsync(string key)
        {
            try
            {
                return await _database.StringIncrementAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing value in Redis for key {Key}", key);
                throw;
            }
        }

        public async Task<long> DecrementAsync(string key)
        {
            try
            {
                return await _database.StringDecrementAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrementing value in Redis for key {Key}", key);
                throw;
            }
        }
    }
}