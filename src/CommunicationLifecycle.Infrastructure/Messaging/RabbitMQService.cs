using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using CommunicationLifecycle.Core.Events;

namespace CommunicationLifecycle.Infrastructure.Messaging;

public class RabbitMQService : IRabbitMQService, IDisposable
{
    private readonly ILogger<RabbitMQService> _logger;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly string _exchangeName = "communication-events";
    private readonly string _queueName = "communication-status-updates";
    private readonly string _routingKey = "status.changed";
    private bool _disposed = false;

    public RabbitMQService(ILogger<RabbitMQService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    private async Task EnsureConnectionAsync()
    {
        if (_connection == null || !_connection.IsOpen)
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration.GetConnectionString("RabbitMQ:Host") ?? "localhost",
                Port = int.Parse(_configuration.GetConnectionString("RabbitMQ:Port") ?? "5672"),
                UserName = _configuration.GetConnectionString("RabbitMQ:Username") ?? "guest",
                Password = _configuration.GetConnectionString("RabbitMQ:Password") ?? "guest",
                VirtualHost = _configuration.GetConnectionString("RabbitMQ:VirtualHost") ?? "/"
            };

            try
            {
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                // Declare exchange
                await _channel.ExchangeDeclareAsync(
                    exchange: _exchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false);

                // Declare queue
                await _channel.QueueDeclareAsync(
                    queue: _queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false);

                // Bind queue to exchange
                await _channel.QueueBindAsync(
                    queue: _queueName,
                    exchange: _exchangeName,
                    routingKey: _routingKey);

                _logger.LogInformation("RabbitMQ connection established successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to RabbitMQ");
                throw;
            }
        }
    }

    public async Task PublishStatusChangedEventAsync(CommunicationStatusChangedEvent statusChangedEvent)
    {
        try
        {
            await EnsureConnectionAsync();

            if (_channel == null)
            {
                throw new InvalidOperationException("RabbitMQ channel is not available");
            }

            var message = JsonSerializer.Serialize(statusChangedEvent);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = new BasicProperties
            {
                Persistent = true,
                MessageId = Guid.NewGuid().ToString(),
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await _channel.BasicPublishAsync(
                exchange: _exchangeName,
                routingKey: _routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Published status changed event for Communication {CommunicationId} to status {NewStatus}",
                statusChangedEvent.CommunicationId, statusChangedEvent.NewStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish status changed event for Communication {CommunicationId}",
                statusChangedEvent.CommunicationId);
            throw;
        }
    }

    public async Task StartConsumingAsync()
    {
        try
        {
            await EnsureConnectionAsync();

            if (_channel == null)
            {
                throw new InvalidOperationException("RabbitMQ channel is not available");
            }

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    
                    var statusChangedEvent = JsonSerializer.Deserialize<CommunicationStatusChangedEvent>(message);
                    
                    if (statusChangedEvent != null)
                    {
                        _logger.LogInformation("Received status changed event for Communication {CommunicationId} to status {NewStatus}",
                            statusChangedEvent.CommunicationId, statusChangedEvent.NewStatus);

                        // Here you would typically call a service to process the event
                        // For now, we'll just log it
                        await ProcessStatusChangedEventAsync(statusChangedEvent);

                        // Acknowledge the message
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                    // Reject the message and don't requeue
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: _queueName,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("Started consuming messages from RabbitMQ queue: {QueueName}", _queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start consuming messages from RabbitMQ");
            throw;
        }
    }

    public async Task StopConsumingAsync()
    {
        try
        {
            if (_channel != null && _channel.IsOpen)
            {
                await _channel.CloseAsync();
            }

            if (_connection != null && _connection.IsOpen)
            {
                await _connection.CloseAsync();
            }

            _logger.LogInformation("Stopped consuming messages from RabbitMQ");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping RabbitMQ consumer");
        }
    }

    private async Task ProcessStatusChangedEventAsync(CommunicationStatusChangedEvent statusChangedEvent)
    {
        // This method would be implemented by a service that handles the business logic
        // For now, just log the event processing
        _logger.LogInformation("Processing status change event: Communication {CommunicationId} changed to {NewStatus} at {Timestamp}",
            statusChangedEvent.CommunicationId, statusChangedEvent.NewStatus, statusChangedEvent.TimestampUtc);

        await Task.CompletedTask;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _channel?.Dispose();
            _connection?.Dispose();
            _disposed = true;
        }
    }
} 