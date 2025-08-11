namespace CommunicationLifecycle.Core.Events;

public class CommunicationStatusChangedEvent
{
    public int CommunicationId { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; }
    public string? Notes { get; set; }
    public string EventType { get; set; } = string.Empty; // e.g., "IdCardPrinted", "EOBShipped"
} 