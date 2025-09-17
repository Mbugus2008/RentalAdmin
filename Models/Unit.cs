using System.ComponentModel.DataAnnotations;

namespace Rental.Models;

public class Unit
{
    public int Id { get; set; }

    [Required]
    public int PropertyId { get; set; }

    public Property? Property { get; set; }

    [Required]
    [StringLength(60)]
    [Display(Name = "Unit")]
    public string UnitNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Unit Type")]
    public string UnitType { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Notes { get; set; }
}
