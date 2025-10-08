using Azure.Messaging.ServiceBus;
using FoodDeliveryApp.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FoodDeliveryApp.Infrastructure.Services
{
    public class ServiceBusPublisher : IServiceBusPublisher, IAsyncDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ILogger<ServiceBusPublisher> _logger;
        private readonly Dictionary<string, ServiceBusSender> _senders;

        public ServiceBusPublisher(
            IConfiguration configuration,
            ILogger<ServiceBusPublisher> logger)
        {
            var connectionString = configuration["ServiceBus:ConnectionString"];
            _client = new ServiceBusClient(connectionString);
            _logger = logger;
            _senders = new Dictionary<string, ServiceBusSender>();
        }

        public async Task SendMessageAsync<T>(string queueOrTopicName, T message)
        {
            try
            {
                var sender = GetOrCreateSender(queueOrTopicName);
                var messageBody = JsonSerializer.Serialize(message);
                var serviceBusMessage = new ServiceBusMessage(messageBody)
                {
                    ContentType = "application/json",
                    MessageId = Guid.NewGuid().ToString()
                };

                await sender.SendMessageAsync(serviceBusMessage);

                _logger.LogInformation(
                    "Sent message to {QueueOrTopic}",
                    queueOrTopicName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send message to {QueueOrTopic}",
                    queueOrTopicName);
                throw;
            }
        }

        public async Task SendMessageAsync<T>(string queueOrTopicName, T message, string sessionId)
        {
            try
            {
                var sender = GetOrCreateSender(queueOrTopicName);
                var messageBody = JsonSerializer.Serialize(message);
                var serviceBusMessage = new ServiceBusMessage(messageBody)
                {
                    ContentType = "application/json",
                    MessageId = Guid.NewGuid().ToString(),
                    SessionId = sessionId
                };

                await sender.SendMessageAsync(serviceBusMessage);

                _logger.LogInformation(
                    "Sent message to {QueueOrTopic} with session {SessionId}",
                    queueOrTopicName,
                    sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send message to {QueueOrTopic}",
                    queueOrTopicName);
                throw;
            }
        }

        public async Task SendMessagesAsync<T>(string queueOrTopicName, IEnumerable<T> messages)
        {
            try
            {
                var sender = GetOrCreateSender(queueOrTopicName);
                var serviceBusMessages = messages.Select(msg =>
                {
                    var messageBody = JsonSerializer.Serialize(msg);
                    return new ServiceBusMessage(messageBody)
                    {
                        ContentType = "application/json",
                        MessageId = Guid.NewGuid().ToString()
                    };
                }).ToList();

                await sender.SendMessagesAsync(serviceBusMessages);

                _logger.LogInformation(
                    "Sent {Count} messages to {QueueOrTopic}",
                    serviceBusMessages.Count,
                    queueOrTopicName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send messages to {QueueOrTopic}",
                    queueOrTopicName);
                throw;
            }
        }

        private ServiceBusSender GetOrCreateSender(string queueOrTopicName)
        {
            if (!_senders.ContainsKey(queueOrTopicName))
            {
                _senders[queueOrTopicName] = _client.CreateSender(queueOrTopicName);
            }
            return _senders[queueOrTopicName];
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var sender in _senders.Values)
            {
                await sender.DisposeAsync();
            }
            await _client.DisposeAsync();
        }
    }
}