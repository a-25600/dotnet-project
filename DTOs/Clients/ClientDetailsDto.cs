using RealEstateApi.DTOs.Common;

namespace RealEstateApi.DTOs.Clients;


public class ClientDetailsDto : ClientDto
{
    public List<DealSummaryDto> Deals { get; set; } = new();
}