using System.ComponentModel.DataAnnotations;

namespace RealEstateApi.DTOs;

public class PropertyDto
{
    [Required(ErrorMessage = "Address is requied")]
    [StringLength(200, MinimumLength = 5)]
    public string Address { get; set; } = string.Empty;

    [Range(100, 10000000, ErrorMessage = "Invalid price")]
    public decimal Price { get; set; }

    public string Type { get; set; } = string.Empty;

    [Range(1, 20, ErrorMessage = "Rooms number must be from 1 to 20")]
    public int RoomsCount { get; set; }

    public bool IsAvailable { get; set; } = true;
}

public class PropertyQueryParameters
{
    public decimal? MaxPrice { get; set; }
    public int? MinRooms { get; set; }
    public bool? IsAvailable { get; set; }
}