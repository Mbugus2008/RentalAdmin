using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Rental.Models;

public class Tenant
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }

    [Phone]
    public string? Phone { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Move-in Date")]
    public DateTime? MoveInDate { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();

    public ICollection<Lease> Leases { get; set; } = new List<Lease>();
}
