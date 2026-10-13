using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateApi.Models;

public class Property
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Address is required")]
    [StringLength(150, MinimumLength = 5)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [Range(100, 10000000, ErrorMessage = "Invalid price")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [StringLength(50)]
    public string Type { get; set; } = string.Empty;

    [Range(1, 20, ErrorMessage = "Rooms number must be from 1 to 20")]
    public int RoomsCount { get; set; }

    public bool IsAvailable { get; set; } = true;
}
