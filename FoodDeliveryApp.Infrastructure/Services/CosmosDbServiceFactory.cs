using FoodDeliveryApp.Core.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Configuration;

namespace FoodDeliveryApp.Infrastructure.Services
{
    public class CosmosDbServiceFactory
    {
        private readonly CosmosClient _cosmosClient;
        private readonly string _databaseName;

        public CosmosDbServiceFactory(IConfiguration configuration)
        {
            var connectionString = configuration["CosmosDb:ConnectionString"];
            _databaseName = configuration["CosmosDb:DatabaseName"];

            var clientOptions = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                },
                ConnectionMode = ConnectionMode.Direct,
                MaxRetryAttemptsOnRateLimitedRequests = 3,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30)
            };

            _cosmosClient = new CosmosClient(connectionString, clientOptions);
        }

        public ICosmosDbService<T> CreateService<T>(string containerName) where T : class
        {
            return new CosmosDbService<T>(_cosmosClient, _databaseName, containerName);
        }
    }
}