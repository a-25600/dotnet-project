using Microsoft.EntityFrameworkCore;
using RealEstateApi.Data;
using RealEstateApi.DTOs.Clients;
using RealEstateApi.Mappings;
using RealEstateApi.Services.Common;
using RealEstateApi.Services.Interfaces;

namespace RealEstateApi.Services.Implementations;

public class ClientService : IClientService
{
    private readonly ApplicationDbContext _context;

    public ClientService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ClientDto>> GetAllAsync(ClientQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        var query = _context.Clients.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var term = parameters.Search.Trim().ToLower();
            query = query.Where(c =>
                c.FullName.ToLower().Contains(term) ||
                c.Email.ToLower().Contains(term) ||
                c.PhoneNumber.ToLower().Contains(term));
        }

        var descending = string.Equals(parameters.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        query = (parameters.SortBy?.ToLower()) switch
        {
            "id" => descending ? query.OrderByDescending(c => c.Id) : query.OrderBy(c => c.Id),
            "email" => descending ? query.OrderByDescending(c => c.Email) : query.OrderBy(c => c.Email),
            "phonenumber" or "phone" => descending
                ? query.OrderByDescending(c => c.PhoneNumber)
                : query.OrderBy(c => c.PhoneNumber),
            _ => descending
                ? query.OrderByDescending(c => c.FullName)
                : query.OrderBy(c => c.FullName)
        };

        var clients = await query.ToListAsync(cancellationToken);
        return clients.Select(c => c.ToDto()).ToList();
    }

    public async Task<ServiceResult<ClientDetailsDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var client = await _context.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (client == null)
            return ServiceResult<ClientDetailsDto>.NotFound($"Client with id {id} not found");

        var deals = await _context.Deals
            .AsNoTracking()
            .Where(d => d.ClientId == id)
            .OrderByDescending(d => d.Date)
            .Select(MappingExtensions.DealToSummary)
            .ToListAsync(cancellationToken);

        return ServiceResult<ClientDetailsDto>.Ok(client.ToDetailsDto(deals));
    }

    public async Task<ServiceResult<ClientDto>> CreateAsync(CreateClientDto dto, CancellationToken cancellationToken = default)
    {
        var email = MappingExtensions.NormalizeEmail(dto.Email);

        if (await _context.Clients.AnyAsync(c => c.Email.ToLower() == email, cancellationToken))
            return ServiceResult<ClientDto>.Conflict($"Client with email {email} already exists");

        var newClient = dto.ToEntity();

        await _context.Clients.AddAsync(newClient, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult<ClientDto>.Created(newClient.ToDto());
    }

    public async Task<ServiceResult> UpdateAsync(int id, UpdateClientDto dto, CancellationToken cancellationToken = default)
    {
        var client = await _context.Clients.FindAsync([id], cancellationToken);

        if (client == null)
            return ServiceResult.NotFound($"Client with id {id} not found");

        var email = MappingExtensions.NormalizeEmail(dto.Email);

        if (await _context.Clients.AnyAsync(c => c.Id != id && c.Email.ToLower() == email, cancellationToken))
            return ServiceResult.Conflict($"Client with email {email} already exists");

        client.UpdateFrom(dto);
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var client = await _context.Clients.FindAsync([id], cancellationToken);

        if (client == null)
            return ServiceResult.NotFound($"Client with id {id} not found");

        if (await _context.Deals.AnyAsync(d => d.ClientId == id, cancellationToken))
            return ServiceResult.Conflict("Client has deals and cannot be deleted");

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult.Ok();
    }
}
