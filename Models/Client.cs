using System.ComponentModel.DataAnnotations;

namespace RealEstateApi.Models;

public class Client
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "FullName is required")]
    [StringLength(100, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;
}
