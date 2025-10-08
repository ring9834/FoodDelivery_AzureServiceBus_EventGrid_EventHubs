using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using FoodDeliveryApp.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FoodDeliveryApp.Infrastructure.Services
{
    public class EventHubPublisher : IEventHubPublisher, IAsyncDisposable
    {
        private readonly EventHubProducerClient _producerClient;
        private readonly ILogger<EventHubPublisher> _logger;

        public EventHubPublisher(
            IConfiguration configuration,
            ILogger<EventHubPublisher> logger)
        {
            var connectionString = configuration["EventHub:ConnectionString"];
            var eventHubName = configuration["EventHub:EventHubName"];

            _producerClient = new EventHubProducerClient(connectionString, eventHubName);
            _logger = logger;
        }

        public async Task SendEventAsync<T>(T eventData)
        {
            try
            {
                var json = JsonSerializer.Serialize(eventData);
                var eventDataBatch = await _producerClient.CreateBatchAsync();

                if (!eventDataBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(json))))
                {
                    throw new Exception("Event is too large for the batch");
                }

                await _producerClient.SendAsync(eventDataBatch);

                _logger.LogInformation("Sent event to Event Hub");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send event to Event Hub");
                throw;
            }
        }

        public async Task SendEventsAsync<T>(IEnumerable<T> eventData)
        {
            try
            {
                var eventDataBatch = await _producerClient.CreateBatchAsync();

                foreach (var data in eventData)
                {
                    var json = JsonSerializer.Serialize(data);
                    var eventDataItem = new EventData(Encoding.UTF8.GetBytes(json));

                    if (!eventDataBatch.TryAdd(eventDataItem))
                    {
                        // Batch is full, send it
                        await _producerClient.SendAsync(eventDataBatch);

                        // Create new batch
                        eventDataBatch = await _producerClient.CreateBatchAsync();

                        if (!eventDataBatch.TryAdd(eventDataItem))
                        {
                            throw new Exception("Event is too large for the batch");
                        }
                    }
                }

                // Send remaining events
                if (eventDataBatch.Count > 0)
                {
                    await _producerClient.SendAsync(eventDataBatch);
                }

                _logger.LogInformation("Sent batch of events to Event Hub");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send events to Event Hub");
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _producerClient.DisposeAsync();
        }
    }
}