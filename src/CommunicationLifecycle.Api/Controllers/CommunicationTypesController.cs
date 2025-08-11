using Microsoft.AspNetCore.Mvc;
using CommunicationLifecycle.Core.Entities;
using CommunicationLifecycle.Core.Enums;
using CommunicationLifecycle.Infrastructure.Repositories;

namespace CommunicationLifecycle.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommunicationTypesController : ControllerBase
{
    private readonly ICommunicationTypeRepository _communicationTypeRepository;
    private readonly ILogger<CommunicationTypesController> _logger;

    public CommunicationTypesController(
        ICommunicationTypeRepository communicationTypeRepository,
        ILogger<CommunicationTypesController> logger)
    {
        _communicationTypeRepository = communicationTypeRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all communication types
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CommunicationType>>> GetCommunicationTypes([FromQuery] bool activeOnly = false)
    {
        try
        {
            var communicationTypes = activeOnly 
                ? await _communicationTypeRepository.GetActiveAsync()
                : await _communicationTypeRepository.GetAllAsync();

            return Ok(communicationTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving communication types");
            return StatusCode(500, "An error occurred while retrieving communication types");
        }
    }

    /// <summary>
    /// Get a specific communication type with its valid statuses
    /// </summary>
    [HttpGet("{typeCode}")]
    public async Task<ActionResult<CommunicationType>> GetCommunicationType(string typeCode)
    {
        try
        {
            var communicationType = await _communicationTypeRepository.GetByCodeWithStatusesAsync(typeCode);
            
            if (communicationType == null)
            {
                return NotFound($"Communication type '{typeCode}' not found");
            }

            return Ok(communicationType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving communication type {TypeCode}", typeCode);
            return StatusCode(500, "An error occurred while retrieving the communication type");
        }
    }

    /// <summary>
    /// Get valid statuses for a communication type
    /// </summary>
    [HttpGet("{typeCode}/statuses")]
    public async Task<ActionResult<IEnumerable<string>>> GetValidStatuses(string typeCode)
    {
        try
        {
            var exists = await _communicationTypeRepository.ExistsAsync(typeCode);
            if (!exists)
            {
                return NotFound($"Communication type '{typeCode}' not found");
            }

            var validStatuses = await _communicationTypeRepository.GetValidStatusesForTypeAsync(typeCode);
            return Ok(validStatuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving valid statuses for type {TypeCode}", typeCode);
            return StatusCode(500, "An error occurred while retrieving valid statuses");
        }
    }

    /// <summary>
    /// Create a new communication type
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CommunicationType>> CreateCommunicationType([FromBody] CreateCommunicationTypeRequest request)
    {
        try
        {
            // Check if type code already exists
            var exists = await _communicationTypeRepository.ExistsAsync(request.TypeCode);
            if (exists)
            {
                return Conflict($"Communication type '{request.TypeCode}' already exists");
            }

            var communicationType = new CommunicationType
            {
                TypeCode = request.TypeCode,
                DisplayName = request.DisplayName,
                Description = request.Description,
                IsActive = request.IsActive
            };

            var createdType = await _communicationTypeRepository.AddAsync(communicationType);

            return CreatedAtAction(nameof(GetCommunicationType), new { typeCode = createdType.TypeCode }, createdType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating communication type");
            return StatusCode(500, "An error occurred while creating the communication type");
        }
    }

    /// <summary>
    /// Update a communication type
    /// </summary>
    [HttpPut("{typeCode}")]
    public async Task<ActionResult<CommunicationType>> UpdateCommunicationType(string typeCode, [FromBody] UpdateCommunicationTypeRequest request)
    {
        try
        {
            var communicationType = await _communicationTypeRepository.GetByCodeAsync(typeCode);
            if (communicationType == null)
            {
                return NotFound($"Communication type '{typeCode}' not found");
            }

            communicationType.DisplayName = request.DisplayName;
            communicationType.Description = request.Description;
            communicationType.IsActive = request.IsActive;

            var updatedType = await _communicationTypeRepository.UpdateAsync(communicationType);
            return Ok(updatedType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating communication type {TypeCode}", typeCode);
            return StatusCode(500, "An error occurred while updating the communication type");
        }
    }

    /// <summary>
    /// Delete a communication type
    /// </summary>
    [HttpDelete("{typeCode}")]
    public async Task<ActionResult> DeleteCommunicationType(string typeCode)
    {
        try
        {
            var exists = await _communicationTypeRepository.ExistsAsync(typeCode);
            if (!exists)
            {
                return NotFound($"Communication type '{typeCode}' not found");
            }

            await _communicationTypeRepository.DeleteAsync(typeCode);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting communication type {TypeCode}", typeCode);
            return StatusCode(500, "An error occurred while deleting the communication type");
        }
    }

    /// <summary>
    /// Get all available status codes
    /// </summary>
    [HttpGet("available-statuses")]
    public ActionResult<AvailableStatusesResponse> GetAvailableStatuses()
    {
        try
        {
            var statuses = CommunicationStatus.All.Select(status => new StatusInfo
            {
                Code = status,
                Description = CommunicationStatus.Descriptions.TryGetValue(status, out var desc) ? desc : status
            }).ToList();

            var response = new AvailableStatusesResponse
            {
                Statuses = statuses
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available statuses");
            return StatusCode(500, "An error occurred while retrieving available statuses");
        }
    }
}

// DTOs
public class CreateCommunicationTypeRequest
{
    public string TypeCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateCommunicationTypeRequest
{
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class AvailableStatusesResponse
{
    public List<StatusInfo> Statuses { get; set; } = new();
}

public class StatusInfo
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
} 