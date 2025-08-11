using System.ComponentModel.DataAnnotations;

namespace CommunicationLifecycle.Core.Entities;

public class Communication
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string TypeCode { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string CurrentStatus { get; set; } = string.Empty;
    
    public DateTime LastUpdatedUtc { get; set; }
    
    public DateTime CreatedUtc { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(200)]
    public string? SourceFileUrl { get; set; }
    
    // Navigation properties
    public virtual ICollection<CommunicationStatusHistory> StatusHistory { get; set; } = new List<CommunicationStatusHistory>();
} 