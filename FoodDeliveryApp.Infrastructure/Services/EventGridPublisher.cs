using Azure;
using Azure.Messaging;
using Azure.Messaging.EventGrid;
using FoodDeliveryApp.Core.Interfaces;
using FoodDeliveryApp.Domain.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FoodDeliveryApp.Infrastructure.Services
{
    public class EventGridPublisher : IEventGridPublisher
    {
        private readonly EventGridPublisherClient _client;
        private readonly ILogger<EventGridPublisher> _logger;

        public EventGridPublisher(
            IConfiguration configuration,
            ILogger<EventGridPublisher> logger)
        {
            var endpoint = configuration["EventGrid:TopicEndpoint"];
            var accessKey = configuration["EventGrid:AccessKey"];

            _client = new EventGridPublisherClient(
                new Uri(endpoint),
                new AzureKeyCredential(accessKey));

            _logger = logger;
        }

        public async Task PublishEventAsync<T>(T domainEvent) where T : DomainEvent
        {
            try
            {
                var cloudEvent = CreateCloudEvent(domainEvent);
                await _client.SendEventAsync(cloudEvent);

                _logger.LogInformation(
                    "Published event {EventType} with ID {EventId}",
                    domainEvent.EventType,
                    domainEvent.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to publish event {EventType} with ID {EventId}",
                    domainEvent.EventType,
                    domainEvent.EventId);
                throw;
            }
        }

        public async Task PublishEventsAsync<T>(IEnumerable<T> domainEvents) where T : DomainEvent
        {
            try
            {
                var cloudEvents = domainEvents.Select(CreateCloudEvent).ToList();
                await _client.SendEventsAsync(cloudEvents);

                _logger.LogInformation(
                    "Published {Count} events",
                    cloudEvents.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish batch of events");
                throw;
            }
        }

        private CloudEvent CreateCloudEvent<T>(T domainEvent) where T : DomainEvent
        {
            return new CloudEvent(
                source: "/fooddeliveryapp",
                type: domainEvent.EventType,
                jsonSerializableData: domainEvent)
            {
                Id = domainEvent.EventId,
                Time = domainEvent.OccurredAt,
                Subject = domainEvent.Subject
            };
        }
    }
}