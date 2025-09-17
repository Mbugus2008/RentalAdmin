using System.ComponentModel.DataAnnotations;

namespace Rental.Models;

public class Unit
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Property")]
    public int PropertyId { get; set; }

    public Property? Property { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Unit Type")]
    public string UnitType { get; set; } = string.Empty;
}
