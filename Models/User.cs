using System;
using System.ComponentModel.DataAnnotations;

namespace Rental.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [StringLength(50)]
    public string? PhoneNumber { get; set; }

    [Required]
    [StringLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    [StringLength(50)]
    public string Role { get; set; } = "Landlord";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
