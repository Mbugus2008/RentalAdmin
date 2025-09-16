using System;
using System.ComponentModel.DataAnnotations;

namespace Rental.Models;

public class Payment
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Lease")]
    public int LeaseId { get; set; }

    public Lease? Lease { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Payment Date")]
    public DateTime PaymentDate { get; set; }

    [DataType(DataType.Currency)]
    [Range(0, 100000)]
    public decimal Amount { get; set; }

    [StringLength(100)]
    [Display(Name = "Payment Method")]
    public string? Method { get; set; }

    [StringLength(200)]
    public string? Notes { get; set; }
}
