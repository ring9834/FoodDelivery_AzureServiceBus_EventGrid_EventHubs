using FoodDeliveryApp.Core.Interfaces;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodDeliveryApp.Infrastructure.Services
{
    public class CosmosDbService<T> : ICosmosDbService<T> where T : class
    {
        private readonly Container _container;

        public CosmosDbService(
            CosmosClient cosmosClient,
            string databaseName,
            string containerName)
        {
            _container = cosmosClient.GetContainer(databaseName, containerName);
        }

        public async Task<T> GetItemAsync(string id, string partitionKey)
        {
            try
            {
                var response = await _container.ReadItemAsync<T>(
                    id,
                    new PartitionKey(partitionKey));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<IEnumerable<T>> GetItemsAsync(string queryString)
        {
            var query = _container.GetItemQueryIterator<T>(new QueryDefinition(queryString));
            var results = new List<T>();

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task<T> CreateItemAsync(T item)
        {
            var response = await _container.CreateItemAsync(item);
            return response.Resource;
        }

        public async Task<T> UpdateItemAsync(string id, T item)
        {
            var response = await _container.UpsertItemAsync(item);
            return response.Resource;
        }

        public async Task DeleteItemAsync(string id, string partitionKey)
        {
            await _container.DeleteItemAsync<T>(id, new PartitionKey(partitionKey));
        }
    }
}