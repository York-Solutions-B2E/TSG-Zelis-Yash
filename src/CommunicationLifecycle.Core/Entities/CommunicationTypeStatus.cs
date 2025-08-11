using System.ComponentModel.DataAnnotations;

namespace CommunicationLifecycle.Core.Entities;

public class CommunicationTypeStatus
{
    [MaxLength(50)]
    public string TypeCode { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string StatusCode { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public int DisplayOrder { get; set; }
    
    // Navigation property
    public virtual CommunicationType CommunicationType { get; set; } = null!;
} 