using FoodDeliveryApp.Core.Interfaces;
using FoodDeliveryApp.Domain.Models;
using FoodDeliveryApp.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Food Delivery API",
        Version = "v1",
        Description = "API for Food Delivery Platform"
    });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register Cosmos DB Services
var cosmosFactory = new CosmosDbServiceFactory(builder.Configuration);
builder.Services.AddSingleton<ICosmosDbService<Order>>(
    cosmosFactory.CreateService<Order>("Orders"));
builder.Services.AddSingleton<ICosmosDbService<Vendor>>(
    cosmosFactory.CreateService<Vendor>("Vendors"));
builder.Services.AddSingleton<ICosmosDbService<DeliveryPerson>>(
    cosmosFactory.CreateService<DeliveryPerson>("DeliveryPersons"));
builder.Services.AddSingleton<ICosmosDbService<Customer>>(
    cosmosFactory.CreateService<Customer>("Customers"));

// Register Azure Service Implementations
builder.Services.AddSingleton<IEventGridPublisher, EventGridPublisher>();
builder.Services.AddSingleton<IServiceBusPublisher, ServiceBusPublisher>();
builder.Services.AddSingleton<IEventHubPublisher, EventHubPublisher>();
builder.Services.AddSingleton<IRedisCache, RedisCacheService>();
builder.Services.AddSingleton<ISignalRNotificationService, SignalRNotificationService>();
builder.Services.AddSingleton<INotificationHubService, NotificationHubService>();

// Add logging
builder.Services.AddLogging();

// Add Application Insights (optional)
//object value = builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Food Delivery API V1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();