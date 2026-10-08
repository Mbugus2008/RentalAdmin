using System;
using System.ComponentModel.DataAnnotations;

namespace Rental.Models;

public class RentalDocument
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    public int? PropertyId { get; set; }
    public Property? Property { get; set; }

    public int? UnitId { get; set; }

    public int? TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Expires")]
    public DateTime? ExpirationDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Uploaded")]
    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    [StringLength(20)]
    public string FileType { get; set; } = "PDF";

    public int FileSize { get; set; }

    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
