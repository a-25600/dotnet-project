using System.ComponentModel.DataAnnotations;

namespace RealEstateApi.DTOs.Deals;

public class UpdateDealDto
{
    [Range(1, double.MaxValue)]
    public decimal Price {  get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;
}