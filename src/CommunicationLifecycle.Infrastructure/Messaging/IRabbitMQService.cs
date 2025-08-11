using CommunicationLifecycle.Core.Events;

namespace CommunicationLifecycle.Infrastructure.Messaging;

public interface IRabbitMQService
{
    Task PublishStatusChangedEventAsync(CommunicationStatusChangedEvent statusChangedEvent);
    Task StartConsumingAsync();
    Task StopConsumingAsync();
} 