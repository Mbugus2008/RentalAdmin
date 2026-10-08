using System;
using System.ComponentModel.DataAnnotations;

namespace Rental.Models;

public class Transaction
{
    public int Id { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Date")]
    public DateTime TransactionDate { get; set; }

    [DataType(DataType.Currency)]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    /// <summary>Income or Expense.</summary>
    [Required]
    [StringLength(20)]
    public string Type { get; set; } = "Expense";

    public int? PropertyId { get; set; }
    public Property? Property { get; set; }

    public int? UnitId { get; set; }

    public int? TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
