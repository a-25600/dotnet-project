using RealEstateApi.DTOs.Common;

namespace RealEstateApi.DTOs.Agents;

public class AgentDetailsDto : AgentDto
{
    public List<DealSummaryDto> Deals { get; set; } = new();
}