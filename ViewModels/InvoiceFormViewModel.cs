using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Rental.ViewModels;

/// <summary>Form model used by the invoice Create/Edit pages (header + itemised lines).</summary>
public class InvoiceFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Select a tenant.")]
    [Display(Name = "Tenant")]
    public int? TenantId { get; set; }

    [Display(Name = "Property")]
    public int? PropertyId { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Invoice date")]
    public DateTime InvoiceDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [Display(Name = "Due date")]
    public DateTime? DueDate { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public string Status { get; set; } = "Sent";

    [DataType(DataType.Currency)]
    [Display(Name = "Amount already paid")]
    [Range(0, 100000000)]
    public decimal AmountPaid { get; set; }

    public List<InvoiceLineInput> Lines { get; set; } = new() { new InvoiceLineInput() };

    public class InvoiceLineInput
    {
        public string? Description { get; set; }

        [Range(0, 10000000, ErrorMessage = "Amount must be zero or more.")]
        public decimal Amount { get; set; }
    }
}

/// <summary>Input model for recording a payment against an invoice.</summary>
public class InvoicePaymentViewModel
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public decimal Balance { get; set; }

    [Required]
    [Range(0.01, 100000000, ErrorMessage = "Enter an amount greater than zero.")]
    public decimal Amount { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Payment date")]
    public DateTime PaymentDate { get; set; } = DateTime.Today;

    [Required]
    [Display(Name = "Method")]
    public string Method { get; set; } = "M-PESA";

    [StringLength(500)]
    public string? Notes { get; set; }
}
