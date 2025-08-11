using Microsoft.AspNetCore.Mvc;
using CommunicationLifecycle.Core.Entities;
using CommunicationLifecycle.Core.Events;
using CommunicationLifecycle.Infrastructure.Repositories;
using CommunicationLifecycle.Infrastructure.Messaging;

namespace CommunicationLifecycle.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommunicationsController : ControllerBase
{
    private readonly ICommunicationRepository _communicationRepository;
    private readonly ICommunicationTypeRepository _communicationTypeRepository;
    private readonly IRabbitMQService _rabbitMQService;
    private readonly ILogger<CommunicationsController> _logger;

    public CommunicationsController(
        ICommunicationRepository communicationRepository,
        ICommunicationTypeRepository communicationTypeRepository,
        IRabbitMQService rabbitMQService,
        ILogger<CommunicationsController> logger)
    {
        _communicationRepository = communicationRepository;
        _communicationTypeRepository = communicationTypeRepository;
        _rabbitMQService = rabbitMQService;
        _logger = logger;
    }

    /// <summary>
    /// Get all communications with pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<CommunicationListResponse>> GetCommunications(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] string? typeCode = null,
        [FromQuery] string? status = null)
    {
        try
        {
            IEnumerable<Communication> communications;
            int totalCount;

            if (!string.IsNullOrEmpty(typeCode) && !string.IsNullOrEmpty(status))
            {
                communications = await _communicationRepository.GetByTypeAndStatusAsync(typeCode, status);
                totalCount = await _communicationRepository.GetCountByTypeAsync(typeCode);
            }
            else if (!string.IsNullOrEmpty(typeCode))
            {
                communications = await _communicationRepository.GetByTypeAsync(typeCode);
                totalCount = await _communicationRepository.GetCountByTypeAsync(typeCode);
            }
            else if (!string.IsNullOrEmpty(status))
            {
                communications = await _communicationRepository.GetByStatusAsync(status);
                totalCount = await _communicationRepository.GetTotalCountAsync();
            }
            else
            {
                communications = await _communicationRepository.GetPagedAsync(page, pageSize);
                totalCount = await _communicationRepository.GetTotalCountAsync();
            }

            var response = new CommunicationListResponse
            {
                Communications = communications.ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving communications");
            return StatusCode(500, "An error occurred while retrieving communications");
        }
    }

    /// <summary>
    /// Get a specific communication by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Communication>> GetCommunication(int id)
    {
        try
        {
            var communication = await _communicationRepository.GetByIdAsync(id);
            
            if (communication == null)
            {
                return NotFound($"Communication with ID {id} not found");
            }

            return Ok(communication);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving communication {CommunicationId}", id);
            return StatusCode(500, "An error occurred while retrieving the communication");
        }
    }

    /// <summary>
    /// Get a communication with its status history
    /// </summary>
    [HttpGet("{id}/with-history")]
    public async Task<ActionResult<Communication>> GetCommunicationWithHistory(int id)
    {
        try
        {
            var communication = await _communicationRepository.GetByIdWithHistoryAsync(id);
            
            if (communication == null)
            {
                return NotFound($"Communication with ID {id} not found");
            }

            return Ok(communication);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving communication with history {CommunicationId}", id);
            return StatusCode(500, "An error occurred while retrieving the communication");
        }
    }

    /// <summary>
    /// Create a new communication
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Communication>> CreateCommunication([FromBody] CreateCommunicationRequest request)
    {
        try
        {
            // Validate that the communication type exists and is active
            var communicationType = await _communicationTypeRepository.GetByCodeAsync(request.TypeCode);
            if (communicationType == null || !communicationType.IsActive)
            {
                return BadRequest($"Invalid or inactive communication type: {request.TypeCode}");
            }

            // Validate that the initial status is valid for this type
            var validStatuses = await _communicationTypeRepository.GetValidStatusesForTypeAsync(request.TypeCode);
            if (!validStatuses.Contains(request.CurrentStatus))
            {
                return BadRequest($"Status '{request.CurrentStatus}' is not valid for communication type '{request.TypeCode}'");
            }

            var communication = new Communication
            {
                Title = request.Title,
                TypeCode = request.TypeCode,
                CurrentStatus = request.CurrentStatus,
                Description = request.Description,
                SourceFileUrl = request.SourceFileUrl
            };

            var createdCommunication = await _communicationRepository.AddAsync(communication);

            // Publish event
            var statusChangedEvent = new CommunicationStatusChangedEvent
            {
                CommunicationId = createdCommunication.Id,
                NewStatus = createdCommunication.CurrentStatus,
                TimestampUtc = createdCommunication.CreatedUtc,
                EventType = "CommunicationCreated",
                Notes = "Communication created"
            };

            await _rabbitMQService.PublishStatusChangedEventAsync(statusChangedEvent);

            return CreatedAtAction(nameof(GetCommunication), new { id = createdCommunication.Id }, createdCommunication);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating communication");
            return StatusCode(500, "An error occurred while creating the communication");
        }
    }

    /// <summary>
    /// Update a communication's status
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<ActionResult<Communication>> UpdateCommunicationStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            var communication = await _communicationRepository.GetByIdAsync(id);
            if (communication == null)
            {
                return NotFound($"Communication with ID {id} not found");
            }

            // Validate that the new status is valid for this communication type
            var validStatuses = await _communicationTypeRepository.GetValidStatusesForTypeAsync(communication.TypeCode);
            if (!validStatuses.Contains(request.NewStatus))
            {
                return BadRequest($"Status '{request.NewStatus}' is not valid for communication type '{communication.TypeCode}'");
            }

            var oldStatus = communication.CurrentStatus;
            communication.CurrentStatus = request.NewStatus;
            communication.LastUpdatedUtc = DateTime.UtcNow;

            // Add status history entry
            communication.StatusHistory.Add(new CommunicationStatusHistory
            {
                StatusCode = request.NewStatus,
                OccurredUtc = communication.LastUpdatedUtc,
                Notes = request.Notes ?? $"Status changed from {oldStatus} to {request.NewStatus}"
            });

            var updatedCommunication = await _communicationRepository.UpdateAsync(communication);

            // Publish event
            var statusChangedEvent = new CommunicationStatusChangedEvent
            {
                CommunicationId = communication.Id,
                NewStatus = request.NewStatus,
                TimestampUtc = communication.LastUpdatedUtc,
                EventType = request.EventType ?? "StatusChanged",
                Notes = request.Notes
            };

            await _rabbitMQService.PublishStatusChangedEventAsync(statusChangedEvent);

            return Ok(updatedCommunication);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating communication status {CommunicationId}", id);
            return StatusCode(500, "An error occurred while updating the communication status");
        }
    }

    /// <summary>
    /// Delete a communication
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCommunication(int id)
    {
        try
        {
            var exists = await _communicationRepository.ExistsAsync(id);
            if (!exists)
            {
                return NotFound($"Communication with ID {id} not found");
            }

            await _communicationRepository.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting communication {CommunicationId}", id);
            return StatusCode(500, "An error occurred while deleting the communication");
        }
    }
}

// DTOs
public class CommunicationListResponse
{
    public List<Communication> Communications { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public class CreateCommunicationRequest
{
    public string Title { get; set; } = string.Empty;
    public string TypeCode { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SourceFileUrl { get; set; }
}

public class UpdateStatusRequest
{
    public string NewStatus { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? EventType { get; set; }
} 