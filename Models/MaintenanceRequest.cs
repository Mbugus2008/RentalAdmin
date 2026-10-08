using System;
using System.ComponentModel.DataAnnotations;

namespace Rental.Models;

public class MaintenanceRequest
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    public int? PropertyId { get; set; }
    public Property? Property { get; set; }

    public int? UnitId { get; set; }

    [StringLength(20)]
    public string Priority { get; set; } = "Medium";

    [StringLength(30)]
    public string Status { get; set; } = "Open";

    [StringLength(100)]
    [Display(Name = "Assigned To")]
    public string? AssignedTo { get; set; }

    [DataType(DataType.Currency)]
    [Display(Name = "Estimated Cost")]
    public decimal? EstimatedCost { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Requested")]
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;

    [DataType(DataType.Date)]
    [Display(Name = "Completed")]
    public DateTime? CompletedDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
