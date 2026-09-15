using System.ComponentModel.DataAnnotations;

namespace RealEstateApi.DTOs.Deals;

public class CreateDealDto
{
    [Required]
    public int PropertyId { get; set; }

    [Required]
    public int ClientId {  get; set; }

    [Required]
    public int RealtorId { get; set; }

    [Range(1, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }
}