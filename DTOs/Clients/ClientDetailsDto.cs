using RealEstateApi.DTOs.Deals;

namespace RealEstateApi.DTOs.Clients;

/// <summary>
/// Детальна інформація про клієнта разом з історією угод.
/// </summary>
public class ClientDetailsDto : ClientDto
{
    /// <summary>
    /// Список угод, укладених за участю клієнта.
    /// </summary>
    public List<DealSummaryDto> Deals { get; set; } = [];
}
