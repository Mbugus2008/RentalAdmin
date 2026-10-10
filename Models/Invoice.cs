using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rental.Models;

/// <summary>
/// A tenant invoice made of itemised lines (rent, water, garbage, …).
/// </summary>
public class Invoice
{
    public int Id { get; set; }

    [StringLength(30)]
    [Display(Name = "Invoice No.")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Tenant")]
    public int TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    public int? PropertyId { get; set; }
    public Property? Property { get; set; }

    public int? UnitId { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Invoice Date")]
    public DateTime InvoiceDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [Display(Name = "Due Date")]
    public DateTime? DueDate { get; set; }

    /// <summary>Draft | Sent | Partial | Paid.</summary>
    [StringLength(30)]
    public string Status { get; set; } = "Sent";

    [DataType(DataType.Currency)]
    public decimal TotalAmount { get; set; }

    [DataType(DataType.Currency)]
    public decimal AmountPaid { get; set; }

    [DataType(DataType.MultilineText)]
    [StringLength(1000)]
    public string? Notes { get; set; }

    public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [NotMapped]
    public decimal Balance => TotalAmount - AmountPaid;
}
