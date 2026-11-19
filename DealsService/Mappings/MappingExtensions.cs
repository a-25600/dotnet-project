using System.Linq.Expressions;
using DealsService.DTOs.Deals;
using DealsService.Models;

namespace DealsService.Mappings;

public static class MappingExtensions
{
    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public static readonly Expression<Func<Deal, DealSummaryDto>> DealToSummary = d => new DealSummaryDto
    {
        Id = d.Id,
        PropertyAddress = "Property #" + d.PropertyId,
        ClientName = "Client #" + d.ClientId,
        AgentName = d.RealtorId != null ? "Agent #" + d.RealtorId : string.Empty,
        Price = d.Price,
        Date = d.Date,
        Status = d.Status
    };

    public static readonly Expression<Func<Deal, DealDto>> DealToDtoExpr = d => new DealDto
    {
        Id = d.Id,
        PropertyId = d.PropertyId,
        PropertyAddress = "Property #" + d.PropertyId,
        ClientId = d.ClientId,
        ClientName = "Client #" + d.ClientId,
        RealtorId = d.RealtorId,
        RealtorName = d.RealtorId != null ? "Agent #" + d.RealtorId : string.Empty,
        Price = d.Price,
        Date = d.Date,
        Status = d.Status
    };

    public static DealDto ToDto(
        this Deal deal,
        string? propertyAddress = null,
        string? clientName = null,
        string? realtorName = null) => new()
        {
            Id = deal.Id,
            PropertyId = deal.PropertyId,
            PropertyAddress = propertyAddress ?? $"Property #{deal.PropertyId}",
            ClientId = deal.ClientId,
            ClientName = clientName ?? $"Client #{deal.ClientId}",
            RealtorId = deal.RealtorId,
            RealtorName = realtorName ?? (deal.RealtorId.HasValue ? $"Agent #{deal.RealtorId}" : string.Empty),
            Price = deal.Price,
            Date = deal.Date,
            Status = deal.Status
        };

    public static DealSummaryDto ToSummaryDto(
        this Deal deal,
        string? propertyAddress = null,
        string? clientName = null,
        string? agentName = null) => new()
        {
            Id = deal.Id,
            PropertyAddress = propertyAddress ?? $"Property #{deal.PropertyId}",
            ClientName = clientName ?? $"Client #{deal.ClientId}",
            AgentName = agentName ?? (deal.RealtorId.HasValue ? $"Agent #{deal.RealtorId}" : string.Empty),
            Price = deal.Price,
            Date = deal.Date,
            Status = deal.Status
        };
}