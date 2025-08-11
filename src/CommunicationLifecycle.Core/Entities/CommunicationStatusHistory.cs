using System.ComponentModel.DataAnnotations;

namespace CommunicationLifecycle.Core.Entities;

public class CommunicationStatusHistory
{
    public int Id { get; set; }
    
    public int CommunicationId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string StatusCode { get; set; } = string.Empty;
    
    public DateTime OccurredUtc { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    // Navigation property
    public virtual Communication Communication { get; set; } = null!;
} 