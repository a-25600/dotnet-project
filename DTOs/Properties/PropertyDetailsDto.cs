using RealEstateApi.DTOs.Deals;

namespace RealEstateApi.DTOs.Properties;

/// <summary>
/// Детальна інформація про об'єкт нерухомості разом з історією угод.
/// </summary>
public class PropertyDetailsDto : PropertyDto
{
    /// <summary>
    /// Список угод, укладених щодо цього об'єкта нерухомості.
    /// </summary>
    public List<DealSummaryDto> Deals { get; set; } = [];
}
