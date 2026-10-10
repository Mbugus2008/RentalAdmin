using System;
using System.ComponentModel.DataAnnotations;

namespace Rental.Models;

/// <summary>
/// Singleton billing settings shared by every invoice: the water price per
/// unit (m³) and the fixed monthly garbage collection fee.
/// </summary>
public class AppSettings
{
    public int Id { get; set; }

    [DataType(DataType.Currency)]
    [Range(0, 1000000)]
    [Display(Name = "Water price per unit")]
    public decimal WaterRatePerUnit { get; set; } = 150m;

    [DataType(DataType.Currency)]
    [Range(0, 10000000)]
    [Display(Name = "Garbage fee (per month)")]
    public decimal GarbageFee { get; set; } = 500m;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
