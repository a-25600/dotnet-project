using System.Linq.Expressions;
using RealEstateApi.DTOs.Agents;
using RealEstateApi.DTOs.Clients;
using RealEstateApi.DTOs.Deals;
using RealEstateApi.DTOs.Properties;
using RealEstateApi.Models;

namespace RealEstateApi.Mappings;

public static class MappingExtensions
{
    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    // ---------- Deal Expressions & Mappings ----------
    public static readonly Expression<Func<Deal, DealSummaryDto>> DealToSummary = d => new DealSummaryDto
    {
        Id = d.Id,
        PropertyAddress = d.Property != null ? d.Property.Address : string.Empty,
        ClientName = d.Client != null ? d.Client.FullName : string.Empty,
        AgentName = d.Realtor != null ? d.Realtor.FullName : string.Empty,
        Price = d.Price,
        Date = d.Date,
        Status = d.Status
    };

    public static readonly Expression<Func<Deal, DealDto>> DealToDtoExpr = d => new DealDto
    {
        Id = d.Id,
        PropertyId = d.PropertyId,
        PropertyAddress = d.Property != null ? d.Property.Address : string.Empty,
        ClientId = d.ClientId,
        ClientName = d.Client != null ? d.Client.FullName : string.Empty,
        RealtorId = d.RealtorId,
        RealtorName = d.Realtor != null ? d.Realtor.FullName : string.Empty,
        Price = d.Price,
        Date = d.Date,
        Status = d.Status
    };

    public static DealDto ToDto(this Deal deal) => new()
    {
        Id = deal.Id,
        PropertyId = deal.PropertyId,
        PropertyAddress = deal.Property?.Address ?? string.Empty,
        ClientId = deal.ClientId,
        ClientName = deal.Client?.FullName ?? string.Empty,
        RealtorId = deal.RealtorId,
        RealtorName = deal.Realtor?.FullName ?? string.Empty,
        Price = deal.Price,
        Date = deal.Date,
        Status = deal.Status
    };

    public static DealSummaryDto ToSummaryDto(this Deal deal) => new()
    {
        Id = deal.Id,
        PropertyAddress = deal.Property?.Address ?? string.Empty,
        ClientName = deal.Client?.FullName ?? string.Empty,
        AgentName = deal.Realtor?.FullName ?? string.Empty,
        Price = deal.Price,
        Date = deal.Date,
        Status = deal.Status
    };

    // ---------- Property ----------
    public static PropertyDto ToDto(this Property property) => new()
    {
        Id = property.Id,
        Address = property.Address,
        Price = property.Price,
        Type = property.Type,
        RoomsCount = property.RoomsCount,
        IsAvailable = property.IsAvailable
    };

    public static PropertyDetailsDto ToDetailsDto(this Property property, List<DealSummaryDto> deals) => new()
    {
        Id = property.Id,
        Address = property.Address,
        Price = property.Price,
        Type = property.Type,
        RoomsCount = property.RoomsCount,
        IsAvailable = property.IsAvailable,
        Deals = deals
    };

    public static Property ToEntity(this CreatePropertyDto dto) => new()
    {
        Address = dto.Address.Trim(),
        Price = dto.Price,
        Type = dto.Type.Trim(),
        RoomsCount = dto.RoomsCount,
        IsAvailable = dto.IsAvailable
    };

    public static void UpdateFrom(this Property property, UpdatePropertyDto dto)
    {
        property.Address = dto.Address.Trim();
        property.Price = dto.Price;
        property.Type = dto.Type.Trim();
        property.RoomsCount = dto.RoomsCount;
        property.IsAvailable = dto.IsAvailable;
    }

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