using Microsoft.EntityFrameworkCore;
using RealEstateApi.Data;
using RealEstateApi.DTOs.Deals;
using RealEstateApi.Mappings;
using RealEstateApi.Models;
using RealEstateApi.Services.Common;
using RealEstateApi.Services.Interfaces;

namespace RealEstateApi.Services.Implementations;

public class DealService : IDealService
{
    private readonly ApplicationDbContext _context;

    public DealService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DealDto>> GetAllAsync(DealQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        var query = _context.Deals
            .AsNoTracking()
            .Include(d => d.Property)
            .Include(d => d.Client)
            .Include(d => d.Realtor)
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
            .Include(d => d.Property)
            .Include(d => d.Client)
            .Include(d => d.Realtor)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (deal == null)
            return ServiceResult<DealDto>.NotFound($"Deal with id {id} not found");

        return ServiceResult<DealDto>.Ok(deal.ToDto());
    }

    public async Task<ServiceResult<DealDto>> CreateAsync(CreateDealDto dto, CancellationToken cancellationToken = default)
    {
        var property = await _context.Properties.FindAsync([dto.PropertyId], cancellationToken);
        if (property == null)
            return ServiceResult<DealDto>.BadRequest($"Property with id {dto.PropertyId} does not exist");

        if (!property.IsAvailable)
            return ServiceResult<DealDto>.BadRequest($"Property with id {dto.PropertyId} is currently not available");

        var client = await _context.Clients.FindAsync([dto.ClientId], cancellationToken);
        if (client == null)
            return ServiceResult<DealDto>.BadRequest($"Client with id {dto.ClientId} does not exist");

        var realtor = await _context.Agents.FindAsync([dto.RealtorId], cancellationToken);
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

        var resultDto = new DealDto
        {
            Id = newDeal.Id,
            PropertyId = newDeal.PropertyId,
            PropertyAddress = property.Address,
            ClientId = newDeal.ClientId,
            ClientName = client.FullName,
            RealtorId = newDeal.RealtorId,
            RealtorName = realtor.FullName,
            Price = newDeal.Price,
            Date = newDeal.Date,
            Status = newDeal.Status
        };

        return ServiceResult<DealDto>.Created(resultDto);
    }

    public async Task<ServiceResult> UpdateAsync(int id, UpdateDealDto dto, CancellationToken cancellationToken = default)
    {
        var deal = await _context.Deals
            .Include(d => d.Property)
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

        if (deal.Property != null)
        {
            if (matchedStatus == "Completed")
            {
                deal.Property.IsAvailable = false;
            }
            else if (previousStatus == "Completed" && (matchedStatus == "Pending" || matchedStatus == "Cancelled"))
            {
                deal.Property.IsAvailable = true;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var deal = await _context.Deals
            .Include(d => d.Property)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (deal == null)
            return ServiceResult.NotFound($"Deal with id {id} not found");

        if (deal.Status == "Completed" && deal.Property != null)
        {
            var otherCompletedDeals = await _context.Deals
                .AnyAsync(d => d.PropertyId == deal.PropertyId && d.Id != id && d.Status == "Completed", cancellationToken);

            if (!otherCompletedDeals)
            {
                deal.Property.IsAvailable = true;
            }
        }

        _context.Deals.Remove(deal);
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult.Ok();
    }
}
