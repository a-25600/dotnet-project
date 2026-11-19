using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using DealsService.Data;
using DealsService.DTOs.Deals;
using DealsService.Mappings;
using DealsService.Models;
using DealsService.Services.Common;
using DealsService.Services.Interfaces;

namespace DealsService.Services.Implementations;

public class DealService : IDealService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DealService(
        ApplicationDbContext context,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IReadOnlyList<DealDto>> GetAllAsync(DealQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        var query = _context.Deals
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            var status = parameters.Status.Trim().ToLower();
            query = query.Where(d => d.Status.ToLower() == status);
        }

        if (parameters.PropertyId.HasValue)
            query = query.Where(d => d.PropertyId == parameters.PropertyId.Value);

        if (parameters.ClientId.HasValue)
            query = query.Where(d => d.ClientId == parameters.ClientId.Value);

        if (parameters.RealtorId.HasValue)
            query = query.Where(d => d.RealtorId == parameters.RealtorId.Value);

        if (parameters.MinPrice.HasValue)
            query = query.Where(d => d.Price >= parameters.MinPrice.Value);

        if (parameters.MaxPrice.HasValue)
            query = query.Where(d => d.Price <= parameters.MaxPrice.Value);

        var descending = string.Equals(parameters.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        query = (parameters.SortBy?.ToLower()) switch
        {
            "price" => descending ? query.OrderByDescending(d => d.Price) : query.OrderBy(d => d.Price),
            "status" => descending ? query.OrderByDescending(d => d.Status) : query.OrderBy(d => d.Status),
            "id" => descending ? query.OrderByDescending(d => d.Id) : query.OrderBy(d => d.Id),
            _ => descending ? query.OrderByDescending(d => d.Date) : query.OrderBy(d => d.Date)
        };

        var deals = await query.Select(MappingExtensions.DealToDtoExpr).ToListAsync(cancellationToken);
        return deals;
    }

    public async Task<ServiceResult<DealDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var deal = await _context.Deals
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (deal == null)
            return ServiceResult<DealDto>.NotFound($"Deal with id {id} not found");

        return ServiceResult<DealDto>.Ok(deal.ToDto());
    }

    public async Task<ServiceResult<DealDto>> CreateAsync(CreateDealDto dto, CancellationToken cancellationToken = default)
    {
        var catalogClient = CreateAuthorizedClient("CatalogClient");
        var propertyResponse = await catalogClient.GetAsync($"/api/properties/{dto.PropertyId}", cancellationToken);
        if (!propertyResponse.IsSuccessStatusCode)
            return ServiceResult<DealDto>.BadRequest($"Property with id {dto.PropertyId} does not exist");

        var property = await propertyResponse.Content.ReadFromJsonAsync<PropertyExternalDto>(cancellationToken: cancellationToken);
        if (property == null)
            return ServiceResult<DealDto>.BadRequest($"Property with id {dto.PropertyId} does not exist");

        if (!property.IsAvailable)
            return ServiceResult<DealDto>.BadRequest($"Property with id {dto.PropertyId} is currently not available");

        var usersClient = CreateAuthorizedClient("UsersClient");

        var clientResponse = await usersClient.GetAsync($"/api/clients/{dto.ClientId}", cancellationToken);
        if (!clientResponse.IsSuccessStatusCode)
            return ServiceResult<DealDto>.BadRequest($"Client with id {dto.ClientId} does not exist");

        var client = await clientResponse.Content.ReadFromJsonAsync<ClientExternalDto>(cancellationToken: cancellationToken);
        if (client == null)
            return ServiceResult<DealDto>.BadRequest($"Client with id {dto.ClientId} does not exist");

        var agentResponse = await usersClient.GetAsync($"/api/agents/{dto.RealtorId}", cancellationToken);
        if (!agentResponse.IsSuccessStatusCode)
            return ServiceResult<DealDto>.BadRequest($"Agent with id {dto.RealtorId} does not exist");

        var realtor = await agentResponse.Content.ReadFromJsonAsync<AgentExternalDto>(cancellationToken: cancellationToken);
        if (realtor == null)
            return ServiceResult<DealDto>.BadRequest($"Agent with id {dto.RealtorId} does not exist");

        if (!realtor.IsActive)
            return ServiceResult<DealDto>.BadRequest($"Agent with id {dto.RealtorId} is inactive");

        var newDeal = new Deal
        {
            PropertyId = dto.PropertyId,
            ClientId = dto.ClientId,
            RealtorId = dto.RealtorId,
            Price = dto.Price,
            Date = DateTime.UtcNow,
            Status = "Pending"
        };

        await _context.Deals.AddAsync(newDeal, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var resultDto = newDeal.ToDto(property.Address, client.FullName, realtor.FullName);
        return ServiceResult<DealDto>.Created(resultDto);
    }

    public async Task<ServiceResult> UpdateAsync(int id, UpdateDealDto dto, CancellationToken cancellationToken = default)
    {
        var deal = await _context.Deals
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (deal == null)
            return ServiceResult.NotFound($"Deal with id {id} not found");

        var normalizedStatus = dto.Status.Trim();
        var allowedStatuses = new[] { "Pending", "Completed", "Cancelled" };
        var matchedStatus = allowedStatuses.FirstOrDefault(s => string.Equals(s, normalizedStatus, StringComparison.OrdinalIgnoreCase));

        if (matchedStatus == null)
            return ServiceResult.BadRequest($"Status must be one of: {string.Join(", ", allowedStatuses)}");

        var previousStatus = deal.Status;
        deal.Price = dto.Price;
        deal.Status = matchedStatus;

        await _context.SaveChangesAsync(cancellationToken);

        if (matchedStatus == "Completed")
        {
            await UpdatePropertyAvailabilityAsync(deal.PropertyId, false, cancellationToken);
        }
        else if (previousStatus == "Completed" && (matchedStatus == "Pending" || matchedStatus == "Cancelled"))
        {
            await UpdatePropertyAvailabilityAsync(deal.PropertyId, true, cancellationToken);
        }

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var deal = await _context.Deals
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (deal == null)
            return ServiceResult.NotFound($"Deal with id {id} not found");

        var wasCompleted = deal.Status == "Completed";
        var propertyId = deal.PropertyId;

        _context.Deals.Remove(deal);
        await _context.SaveChangesAsync(cancellationToken);

        if (wasCompleted)
        {
            var otherCompletedDeals = await _context.Deals
                .AnyAsync(d => d.PropertyId == propertyId && d.Status == "Completed", cancellationToken);

            if (!otherCompletedDeals)
            {
                await UpdatePropertyAvailabilityAsync(propertyId, true, cancellationToken);
            }
        }

        return ServiceResult.Ok();
    }

    private HttpClient CreateAuthorizedClient(string clientName)
    {
        var client = _httpClientFactory.CreateClient(clientName);
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(authHeader) &&
            AuthenticationHeaderValue.TryParse(authHeader, out var parsedHeader))
        {
            client.DefaultRequestHeaders.Authorization = parsedHeader;
        }

        return client;
    }

    private async Task UpdatePropertyAvailabilityAsync(int propertyId, bool isAvailable, CancellationToken cancellationToken)
    {
        var catalogClient = CreateAuthorizedClient("CatalogClient");
        await catalogClient.PatchAsJsonAsync(
            $"/api/properties/{propertyId}/availability",
            new { IsAvailable = isAvailable },
            cancellationToken);
    }

    private sealed record PropertyExternalDto(int Id, string Address, bool IsAvailable);
    private sealed record ClientExternalDto(int Id, string FullName);
    private sealed record AgentExternalDto(int Id, string FullName, bool IsActive);
}