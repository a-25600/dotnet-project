using System.Linq.Expressions;
using RealEstateApi.DTOs.Agents;
using RealEstateApi.DTOs.Clients;
using RealEstateApi.DTOs.Deals;
using RealEstateApi.Models;

namespace RealEstateApi.Mappings;

public static class MappingExtensions
{
    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public static readonly Expression<Func<Deal, DealSummaryDto>> DealToSummary = d => new DealSummaryDto
    {
        Id = d.Id,
        PropertyAddress = d.Property!.Address,
        ClientName = d.Client!.FullName,
        AgentName = d.Realtor!.FullName,
        Price = d.Price,
        Date = d.Date,
        Status = d.Status
    };

    // ---------- Client ----------
    public static ClientDto ToDto(this Client client) => new()
    {
        Id = client.Id,
        FullName = client.FullName,
        Email = client.Email,
        PhoneNumber = client.PhoneNumber
    };

    public static ClientDetailsDto ToDetailsDto(this Client client, List<DealSummaryDto> deals) => new()
    {
        Id = client.Id,
        FullName = client.FullName,
        Email = client.Email,
        PhoneNumber = client.PhoneNumber,
        Deals = deals
    };

    public static Client ToEntity(this CreateClientDto dto) => new()
    {
        FullName = dto.FullName.Trim(),
        Email = NormalizeEmail(dto.Email),
        PhoneNumber = dto.PhoneNumber.Trim()
    };

    public static void UpdateFrom(this Client client, UpdateClientDto dto)
    {
        client.FullName = dto.FullName.Trim();
        client.Email = NormalizeEmail(dto.Email);
        client.PhoneNumber = dto.PhoneNumber.Trim();
    }

    // ---------- Agent ----------
    public static AgentDto ToDto(this Agent agent) => new()
    {
        Id = agent.Id,
        FullName = agent.FullName,
        Email = agent.Email,
        PhoneNumber = agent.PhoneNumber,
        LicenseNumber = agent.LicenseNumber,
        IsActive = agent.IsActive
    };

    public static AgentDetailsDto ToDetailsDto(this Agent agent, List<DealSummaryDto> deals) => new()
    {
        Id = agent.Id,
        FullName = agent.FullName,
        Email = agent.Email,
        PhoneNumber = agent.PhoneNumber,
        LicenseNumber = agent.LicenseNumber,
        IsActive = agent.IsActive,
        Deals = deals
    };

    public static Agent ToEntity(this CreateAgentDto dto) => new()
    {
        FullName = dto.FullName.Trim(),
        Email = NormalizeEmail(dto.Email),
        PhoneNumber = dto.PhoneNumber.Trim(),
        LicenseNumber = dto.LicenseNumber.Trim(),
        IsActive = true
    };

    public static void UpdateFrom(this Agent agent, UpdateAgentDto dto)
    {
        agent.FullName = dto.FullName.Trim();
        agent.Email = NormalizeEmail(dto.Email);
        agent.PhoneNumber = dto.PhoneNumber.Trim();
        agent.LicenseNumber = dto.LicenseNumber.Trim();
        agent.IsActive = dto.IsActive;
    }
}