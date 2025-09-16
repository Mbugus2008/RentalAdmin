using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Rental.Models;

public class Property
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(50)]
    public string? State { get; set; }

    [Display(Name = "ZIP Code")]
    [StringLength(20)]
    public string? ZipCode { get; set; }

    [Range(1, 500)]
    public int Units { get; set; }

    [Display(Name = "Notes")]
    [DataType(DataType.MultilineText)]
    public string? Notes { get; set; }

    public ICollection<Lease> Leases { get; set; } = new List<Lease>();
}
