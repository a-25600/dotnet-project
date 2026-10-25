using Microsoft.EntityFrameworkCore;
using RealEstateApi.Data;
using RealEstateApi.DTOs.Properties;
using RealEstateApi.Mappings;
using RealEstateApi.Services.Common;
using RealEstateApi.Services.Interfaces;

namespace RealEstateApi.Services.Implementations;

public class PropertyService : IPropertyService
{
    private readonly ApplicationDbContext _context;

    public PropertyService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PropertyDto>> GetAllAsync(PropertyQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        var query = _context.Properties.AsNoTracking().AsQueryable();

        if (parameters.MinPrice.HasValue)
            query = query.Where(p => p.Price >= parameters.MinPrice.Value);

        if (parameters.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= parameters.MaxPrice.Value);

        if (parameters.MinRooms.HasValue)
            query = query.Where(p => p.RoomsCount >= parameters.MinRooms.Value);

        if (parameters.MaxRooms.HasValue)
            query = query.Where(p => p.RoomsCount <= parameters.MaxRooms.Value);

        if (!string.IsNullOrWhiteSpace(parameters.Type))
        {
            var type = parameters.Type.Trim().ToLower();
            query = query.Where(p => p.Type.ToLower() == type);
        }

        if (parameters.IsAvailable.HasValue)
            query = query.Where(p => p.IsAvailable == parameters.IsAvailable.Value);

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var term = parameters.Search.Trim().ToLower();
            query = query.Where(p => p.Address.ToLower().Contains(term) || p.Type.ToLower().Contains(term));
        }

        var descending = string.Equals(parameters.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        query = (parameters.SortBy?.ToLower()) switch
        {
            "price" => descending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "rooms" or "roomscount" => descending ? query.OrderByDescending(p => p.RoomsCount) : query.OrderBy(p => p.RoomsCount),
            "address" => descending ? query.OrderByDescending(p => p.Address) : query.OrderBy(p => p.Address),
            "type" => descending ? query.OrderByDescending(p => p.Type) : query.OrderBy(p => p.Type),
            _ => descending ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id)
        };

        var properties = await query.ToListAsync(cancellationToken);
        return properties.Select(p => p.ToDto()).ToList();
    }

    public async Task<ServiceResult<PropertyDetailsDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var property = await _context.Properties
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (property == null)
            return ServiceResult<PropertyDetailsDto>.NotFound($"Property with id {id} not found");

        var deals = await _context.Deals
            .AsNoTracking()
            .Where(d => d.PropertyId == id)
            .OrderByDescending(d => d.Date)
            .Select(MappingExtensions.DealToSummary)
            .ToListAsync(cancellationToken);

        return ServiceResult<PropertyDetailsDto>.Ok(property.ToDetailsDto(deals));
    }

    public async Task<ServiceResult<PropertyDto>> CreateAsync(CreatePropertyDto dto, CancellationToken cancellationToken = default)
    {
        var newProperty = dto.ToEntity();

        await _context.Properties.AddAsync(newProperty, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult<PropertyDto>.Created(newProperty.ToDto());
    }

    public async Task<ServiceResult> UpdateAsync(int id, UpdatePropertyDto dto, CancellationToken cancellationToken = default)
    {
        var existingProperty = await _context.Properties.FindAsync([id], cancellationToken);

        if (existingProperty == null)
            return ServiceResult.NotFound($"Property with id {id} not found");

        existingProperty.UpdateFrom(dto);
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var property = await _context.Properties.FindAsync([id], cancellationToken);

        if (property == null)
            return ServiceResult.NotFound($"Property with id {id} not found");

        if (await _context.Deals.AnyAsync(d => d.PropertyId == id, cancellationToken))
            return ServiceResult.Conflict("Property has associated deals and cannot be deleted. Change availability status instead.");

        _context.Properties.Remove(property);
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult.Ok();
    }
}
