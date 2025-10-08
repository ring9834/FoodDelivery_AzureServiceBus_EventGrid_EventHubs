using FoodDeliveryApp.Core.Interfaces;
using FoodDeliveryApp.Domain.Models;
using FoodDeliveryApp.Infrastructure.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication() // Use ASP.NET Core integration for Azure Functions
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Register Cosmos DB Services
        var cosmosFactory = new CosmosDbServiceFactory(context.Configuration);
        services.AddSingleton<ICosmosDbService<Order>>(
            cosmosFactory.CreateService<Order>("Orders"));
        services.AddSingleton<ICosmosDbService<Vendor>>(
            cosmosFactory.CreateService<Vendor>("Vendors"));
        services.AddSingleton<ICosmosDbService<DeliveryPerson>>(
            cosmosFactory.CreateService<DeliveryPerson>("DeliveryPersons"));
        services.AddSingleton<ICosmosDbService<Customer>>(
            cosmosFactory.CreateService<Customer>("Customers"));

        // Register Azure Service Implementations
        services.AddSingleton<IEventGridPublisher, EventGridPublisher>();
        services.AddSingleton<IServiceBusPublisher, ServiceBusPublisher>();
        services.AddSingleton<IEventHubPublisher, EventHubPublisher>();
        services.AddSingleton<IRedisCache, RedisCacheService>();
        services.AddSingleton<ISignalRNotificationService, SignalRNotificationService>();
        services.AddSingleton<INotificationHubService, NotificationHubService>();
    })
    .Build();

host.Run();