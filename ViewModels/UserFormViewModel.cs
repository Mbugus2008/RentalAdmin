using System.ComponentModel.DataAnnotations;

namespace Rental.ViewModels;

public class UserFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(50)]
    [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Choose a user type.")]
    [Display(Name = "User type")]
    public string Role { get; set; } = "Agent";

    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "The password must be at least {2} characters long.")]
    [Display(Name = "Password")]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
    public string? ConfirmPassword { get; set; }
}
