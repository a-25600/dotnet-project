using System.ComponentModel.DataAnnotations;

namespace RealEstateApi.Models;

public class Agent
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "FullName is required")]
    [StringLength(100, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone(ErrorMessage = "Invalid phone number")]
    [StringLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LicenseNumber { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
