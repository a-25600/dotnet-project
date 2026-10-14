using RealEstateApi.DTOs.Deals;

namespace RealEstateApi.DTOs.Agents;

public class AgentDetailsDto : AgentDto
{
    public List<DealSummaryDto> Deals { get; set; } = new();
}
