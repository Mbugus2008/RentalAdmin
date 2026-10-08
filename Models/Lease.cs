using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Rental.Models;

public class Lease
{
    public int Id { get; set; }

    [Display(Name = "Property")]
    public int? PropertyId { get; set; }

    public Property? Property { get; set; }

    [Display(Name = "Unit")]
    public int? UnitId { get; set; }

    [Required]
    [Display(Name = "Tenant")]
    public int TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "End Date")]
    public DateTime? EndDate { get; set; }

    [DataType(DataType.Currency)]
    [Range(0, 100000)]
    [Display(Name = "Monthly Rent")]
    public decimal MonthlyRent { get; set; }

    [DataType(DataType.Currency)]
    [Range(0, 100000)]
    [Display(Name = "Security Deposit")]
    public decimal SecurityDeposit { get; set; }

    [DataType(DataType.MultilineText)]
    [StringLength(2000)]
    [Display(Name = "Lease Terms")]
    public string? LeaseTerms { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = "Active";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
