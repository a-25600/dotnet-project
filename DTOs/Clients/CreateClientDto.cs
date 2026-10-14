using System.ComponentModel.DataAnnotations;

namespace RealEstateApi.DTOs.Clients;

public class CreateClientDto
{
    [Required(ErrorMessage = "FullName is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "FullName must be between 3 and 100 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [StringLength(100, ErrorMessage = "Email must be at most 100 characters")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "PhoneNumber is required")]
    [RegularExpression(@"^\+?[1-9]\d{9,14}$", ErrorMessage = "Phone must be in international format, e.g. +380501234567")]
    public string PhoneNumber { get; set; } = string.Empty;
}