using System.ComponentModel.DataAnnotations;

namespace CommunicationLifecycle.Core.Entities;

public class CommunicationType
{
    [Key]
    [MaxLength(50)]
    public string TypeCode { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<CommunicationTypeStatus> TypeStatuses { get; set; } = new List<CommunicationTypeStatus>();
} 