using Microsoft.AspNetCore.Mvc;
using CommunicationLifecycle.Core.Entities;
using CommunicationLifecycle.Core.Events;
using CommunicationLifecycle.Infrastructure.Repositories;
using CommunicationLifecycle.Infrastructure.Messaging;

namespace CommunicationLifecycle.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventSimulatorController : ControllerBase
{
    private readonly ICommunicationRepository _communicationRepository;
    private readonly ICommunicationTypeRepository _communicationTypeRepository;
    private readonly IRabbitMQService _rabbitMQService;
    private readonly ILogger<EventSimulatorController> _logger;

    public EventSimulatorController(
        ICommunicationRepository communicationRepository,
        ICommunicationTypeRepository communicationTypeRepository,
        IRabbitMQService rabbitMQService,
        ILogger<EventSimulatorController> logger)
    {
        _communicationRepository = communicationRepository;
        _communicationTypeRepository = communicationTypeRepository;
        _rabbitMQService = rabbitMQService;
        _logger = logger;
    }

    /// <summary>
    /// Get list of communications for event simulation dropdown
    /// </summary>
    [HttpGet("communications")]
    public async Task<ActionResult<IEnumerable<CommunicationSummary>>> GetCommunicationsForSimulation()
    {
        try
        {
            var communications = await _communicationRepository.GetAllAsync();
            
            var summaries = communications.Select(c => new CommunicationSummary
            {
                Id = c.Id,
                Title = c.Title,
                TypeCode = c.TypeCode,
                CurrentStatus = c.CurrentStatus,
                LastUpdatedUtc = c.LastUpdatedUtc
            }).ToList();

            return Ok(summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving communications for simulation");
            return StatusCode(500, "An error occurred while retrieving communications");
        }
    }

    /// <summary>
    /// Get available events for a specific communication
    /// </summary>
    [HttpGet("communications/{id}/available-events")]
    public async Task<ActionResult<AvailableEventsResponse>> GetAvailableEvents(int id)
    {
        try
        {
            var communication = await _communicationRepository.GetByIdAsync(id);
            if (communication == null)
            {
                return NotFound($"Communication with ID {id} not found");
            }

            var validStatuses = await _communicationTypeRepository.GetValidStatusesForTypeAsync(communication.TypeCode);
            
            var availableEvents = validStatuses
                .Where(status => status != communication.CurrentStatus) // Exclude current status
                .Select(status => new EventOption
                {
                    EventType = GetEventTypeForStatus(communication.TypeCode, status),
                    NewStatus = status,
                    Description = GetEventDescription(communication.TypeCode, status)
                })
                .ToList();

            var response = new AvailableEventsResponse
            {
                CommunicationId = id,
                CurrentStatus = communication.CurrentStatus,
                AvailableEvents = availableEvents
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available events for communication {CommunicationId}", id);
            return StatusCode(500, "An error occurred while retrieving available events");
        }
    }

    /// <summary>
    /// Publish an event to simulate status change
    /// </summary>
    [HttpPost("publish-event")]
    public async Task<ActionResult<SimulateEventResponse>> PublishEvent([FromBody] SimulateEventRequest request)
    {
        try
        {
            var communication = await _communicationRepository.GetByIdAsync(request.CommunicationId);
            if (communication == null)
            {
                return NotFound($"Communication with ID {request.CommunicationId} not found");
            }

            // Validate that the new status is valid for this communication type
            var validStatuses = await _communicationTypeRepository.GetValidStatusesForTypeAsync(communication.TypeCode);
            if (!validStatuses.Contains(request.NewStatus))
            {
                return BadRequest($"Status '{request.NewStatus}' is not valid for communication type '{communication.TypeCode}'");
            }

            // Create and publish the event
            var statusChangedEvent = new CommunicationStatusChangedEvent
            {
                CommunicationId = request.CommunicationId,
                NewStatus = request.NewStatus,
                TimestampUtc = DateTime.UtcNow,
                EventType = request.EventType,
                Notes = request.Notes ?? $"Event simulated via Event Simulator: {request.EventType}"
            };

            await _rabbitMQService.PublishStatusChangedEventAsync(statusChangedEvent);

            // Update the communication status in the database
            var oldStatus = communication.CurrentStatus;
            communication.CurrentStatus = request.NewStatus;
            communication.LastUpdatedUtc = statusChangedEvent.TimestampUtc;

            // Add status history entry
            communication.StatusHistory.Add(new CommunicationStatusHistory
            {
                StatusCode = request.NewStatus,
                OccurredUtc = statusChangedEvent.TimestampUtc,
                Notes = statusChangedEvent.Notes
            });

            await _communicationRepository.UpdateAsync(communication);

            var response = new SimulateEventResponse
            {
                Success = true,
                Message = $"Event '{request.EventType}' published successfully for Communication {request.CommunicationId}",
                OldStatus = oldStatus,
                NewStatus = request.NewStatus,
                TimestampUtc = statusChangedEvent.TimestampUtc
            };

            _logger.LogInformation("Event simulation completed: {EventType} for Communication {CommunicationId} - {OldStatus} -> {NewStatus}",
                request.EventType, request.CommunicationId, oldStatus, request.NewStatus);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing simulated event for communication {CommunicationId}", request.CommunicationId);
            return StatusCode(500, "An error occurred while publishing the event");
        }
    }

    /// <summary>
    /// Get common event types for dropdown
    /// </summary>
    [HttpGet("event-types")]
    public ActionResult<IEnumerable<string>> GetCommonEventTypes()
    {
        try
        {
            var eventTypes = new[]
            {
                "IdCardPrinted",
                "IdCardShipped",
                "IdCardDelivered",
                "EOBGenerated",
                "EOBPrinted",
                "EOBMailed",
                "EOPProcessed",
                "DocumentFailed",
                "DocumentCancelled",
                "PackageReturned",
                "CustomEvent"
            };

            return Ok(eventTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event types");
            return StatusCode(500, "An error occurred while retrieving event types");
        }
    }

    private static string GetEventTypeForStatus(string typeCode, string status)
    {
        // Generate appropriate event type based on communication type and status
        return status switch
        {
            "Printed" when typeCode == "ID_CARD" => "IdCardPrinted",
            "Shipped" when typeCode == "ID_CARD" => "IdCardShipped",
            "Delivered" when typeCode == "ID_CARD" => "IdCardDelivered",
            "Printed" when typeCode == "EOB" => "EOBPrinted",
            "Shipped" when typeCode == "EOB" => "EOBMailed",
            "Delivered" when typeCode == "EOB" => "EOBDelivered",
            "Printed" when typeCode == "EOP" => "EOPPrinted",
            "Failed" => $"{typeCode}ProcessingFailed",
            "Cancelled" => $"{typeCode}Cancelled",
            "Returned" => $"{typeCode}Returned",
            _ => $"{typeCode}{status}"
        };
    }

    private static string GetEventDescription(string typeCode, string status)
    {
        return $"{typeCode} status changed to {status}";
    }
}

// DTOs
public class CommunicationSummary
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string TypeCode { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public DateTime LastUpdatedUtc { get; set; }
}

public class AvailableEventsResponse
{
    public int CommunicationId { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
    public List<EventOption> AvailableEvents { get; set; } = new();
}

public class EventOption
{
    public string EventType { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class SimulateEventRequest
{
    public int CommunicationId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class SimulateEventResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; }
} 