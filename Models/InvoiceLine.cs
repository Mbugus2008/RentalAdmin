using System.ComponentModel.DataAnnotations;

namespace Rental.Models;

/// <summary>One billed item on an <see cref="Invoice"/> (e.g. Water, Garbage).</summary>
public class InvoiceLine
{
    public int Id { get; set; }

    public int InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    [Required]
    [StringLength(120)]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [DataType(DataType.Currency)]
    [Range(0, 10000000)]
    public decimal Amount { get; set; }
}
