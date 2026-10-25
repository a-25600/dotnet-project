using RealEstateApi.DTOs.Deals;

namespace RealEstateApi.DTOs.Agents;

/// <summary>
/// Детальна інформація про ріелтора разом з історією угод.
/// </summary>
public class AgentDetailsDto : AgentDto
{
    /// <summary>
    /// Список угод, проведених ріелтором.
    /// </summary>
    public List<DealSummaryDto> Deals { get; set; } = [];
}
