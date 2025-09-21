using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Events.User.EventHandlers;

namespace Infrastructure.Messaging.RabbitMQ.Exchanges.User
{
    public interface IUserEventPublisher
    {
        Task PublishUserCreatedAsync(UserCreatedEvent userCreatedEvent);
        Task PublishUserProfileUpdatedAsync(UserProfileUpdatedEvent profileUpdatedEvent);
        Task PublishUserEmailVerifiedAsync(UserEmailVerifiedEvent emailVerifiedEvent);
        Task PublishUserPasswordChangedAsync(UserPasswordChangedEvent passwordChangedEvent);
        Task PublishUserRoleAddedAsync(UserRoleAddedEvent roleAddedEvent);
        Task PublishUserRoleRemovedAsync(UserRoleRemovedEvent roleRemovedEvent);
        Task PublishUserSessionCreatedAsync(UserSessionCreatedEvent sessionCreatedEvent);
        Task PublishUserDeactivatedAsync(UserDeactivatedEvent userDeactivatedEvent);
        Task PublishUserReactivatedAsync(UserReactivatedEvent userReactivatedEvent);
    }

    public class UserEventPublisher : IUserEventPublisher
    {
        private readonly IConnection _connection;
        private readonly ILogger<UserEventPublisher> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public UserEventPublisher(IConnection connection, ILogger<UserEventPublisher> logger)
        {
            _connection = connection;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task PublishUserCreatedAsync(UserCreatedEvent userCreatedEvent)
        {
            await PublishEventAsync(
                exchange: "user.events",
                routingKey: "user.created",
                eventData: userCreatedEvent,
                eventType: "UserCreated"
            );
        }

        public async Task PublishUserProfileUpdatedAsync(UserProfileUpdatedEvent profileUpdatedEvent)
        {
            await PublishEventAsync(
                exchange: "user.events",
                routingKey: "user.profile.updated",
                eventData: profileUpdatedEvent,
                eventType: "UserProfileUpdated"
            );
        }

        public async Task PublishUserEmailVerifiedAsync(UserEmailVerifiedEvent emailVerifiedEvent)
        {
            await PublishEventAsync(
                exchange: "user.events",
                routingKey: "user.email.verified",
                eventData: emailVerifiedEvent,
                eventType: "UserEmailVerified"
            );
        }

        public async Task PublishUserPasswordChangedAsync(UserPasswordChangedEvent passwordChangedEvent)
        {
            await PublishEventAsync(
                exchange: "user.events",
                routingKey: "user.password.changed",
                eventData: passwordChangedEvent,
                eventType: "UserPasswordChanged"
            );
        }

        public async Task PublishUserRoleAddedAsync(UserRoleAddedEvent roleAddedEvent)
        {
            await PublishEventAsync(
                exchange: "user.events",
                routingKey: "user.role.added",
                eventData: roleAddedEvent,
                eventType: "UserRoleAdded"
            );
        }

        public async Task PublishUserRoleRemovedAsync(UserRoleRemovedEvent roleRemovedEvent)
        {
            await PublishEventAsync(
                exchange: "user.events",
                routingKey: "user.role.removed",
                eventData: roleRemovedEvent,
                eventType: "UserRoleRemoved"
            );
        }

        public async Task PublishUserSessionCreatedAsync(UserSessionCreatedEvent sessionCreatedEvent)
        {
            await PublishEventAsync(
                exchange: "user.events",
                routingKey: "user.session.created",
                eventData: sessionCreatedEvent,
                eventType: "UserSessionCreated"
            );
        }

        public async Task PublishUserDeactivatedAsync(UserDeactivatedEvent userDeactivatedEvent)
        {
            await PublishEventAsync(
                exchange: "user.events",
                routingKey: "user.deactivated",
                eventData: userDeactivatedEvent,
                eventType: "UserDeactivated"
            );
        }

        public async Task PublishUserReactivatedAsync(UserReactivatedEvent userReactivatedEvent)
        {
            await PublishEventAsync(
                exchange: "user.events",
                routingKey: "user.reactivated",
                eventData: userReactivatedEvent,
                eventType: "UserReactivated"
            );
        }

        private async Task PublishEventAsync<T>(string exchange, string routingKey, T eventData, string eventType)
        {
            try
            {
                using var channel = _connection.CreateModel();
                
                // Declare exchange
                channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Topic, durable: true);

                // Create message envelope
                var messageEnvelope = new MessageEnvelope<T>
                {
                    EventId = Guid.NewGuid(),
                    EventType = eventType,
                    EventVersion = "1.0",
                    Timestamp = DateTime.UtcNow,
                    Data = eventData
                };

                // Serialize message
                var messageBody = JsonSerializer.Serialize(messageEnvelope, _jsonOptions);
                var body = Encoding.UTF8.GetBytes(messageBody);

                // Set message properties
                var properties = channel.CreateBasicProperties();
                properties.MessageId = messageEnvelope.EventId.ToString();
                properties.Timestamp = new AmqpTimestamp(((DateTimeOffset)messageEnvelope.Timestamp).ToUnixTimeSeconds());
                properties.Type = eventType;
                properties.Persistent = true;
                properties.Headers = new System.Collections.Generic.Dictionary<string, object>
                {
                    ["source"] = "user-service",
                    ["version"] = "1.0.0"
                };

                // Publish message
                channel.BasicPublish(
                    exchange: exchange,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body
                );

                _logger.LogInformation(
                    "Published {EventType} event for user {UserId} to exchange {Exchange} with routing key {RoutingKey}",
                    eventType,
                    GetUserIdFromEvent(eventData),
                    exchange,
                    routingKey
                );

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to publish {EventType} event for user {UserId} to exchange {Exchange}",
                    eventType,
                    GetUserIdFromEvent(eventData),
                    exchange
                );
                throw;
            }
        }

        private string GetUserIdFromEvent<T>(T eventData)
        {
            // Use reflection to get UserId from event data
            var userIdProperty = typeof(T).GetProperty("UserId");
            if (userIdProperty != null)
            {
                var userId = userIdProperty.GetValue(eventData);
                return userId?.ToString() ?? "unknown";
            }
            return "unknown";
        }
    }

    public class MessageEnvelope<T>
    {
        public Guid EventId { get; set; }
        public string EventType { get; set; }
        public string EventVersion { get; set; }
        public DateTime Timestamp { get; set; }
        public T Data { get; set; }
    }
}
