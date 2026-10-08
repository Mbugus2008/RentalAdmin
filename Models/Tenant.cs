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

    [StringLength(100)]
    [Display(Name = "Emergency Contact")]
    public string? EmergencyContactName { get; set; }

    [Phone]
    [StringLength(50)]
    [Display(Name = "Emergency Phone")]
    public string? EmergencyContactPhone { get; set; }

    [DataType(DataType.MultilineText)]
    [StringLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();

    public ICollection<Lease> Leases { get; set; } = new List<Lease>();
}
