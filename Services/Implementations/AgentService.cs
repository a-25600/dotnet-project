using Microsoft.EntityFrameworkCore;
using RealEstateApi.Data;
using RealEstateApi.DTOs.Agents;
using RealEstateApi.Mappings;
using RealEstateApi.Services.Common;
using RealEstateApi.Services.Interfaces;

namespace RealEstateApi.Services.Implementations;

public class AgentService : IAgentService
{
    private readonly ApplicationDbContext _context;

    public AgentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AgentDto>> GetAllAsync(AgentQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        var query = _context.Agents.AsNoTracking().AsQueryable();

        if (parameters.IsActive.HasValue)
            query = query.Where(a => a.IsActive == parameters.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var term = parameters.Search.Trim().ToLower();
            query = query.Where(a =>
                a.FullName.ToLower().Contains(term) ||
                a.Email.ToLower().Contains(term) ||
                a.PhoneNumber.ToLower().Contains(term) ||
                a.LicenseNumber.ToLower().Contains(term));
        }

        var descending = string.Equals(parameters.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        query = (parameters.SortBy?.ToLower()) switch
        {
            "id" => descending ? query.OrderByDescending(a => a.Id) : query.OrderBy(a => a.Id),
            "email" => descending ? query.OrderByDescending(a => a.Email) : query.OrderBy(a => a.Email),
            "phonenumber" or "phone" => descending
                ? query.OrderByDescending(a => a.PhoneNumber)
                : query.OrderBy(a => a.PhoneNumber),
            "licensenumber" or "license" => descending
                ? query.OrderByDescending(a => a.LicenseNumber)
                : query.OrderBy(a => a.LicenseNumber),
            "isactive" => descending
                ? query.OrderByDescending(a => a.IsActive)
                : query.OrderBy(a => a.IsActive),
            _ => descending
                ? query.OrderByDescending(a => a.FullName)
                : query.OrderBy(a => a.FullName)
        };

        var agents = await query.ToListAsync(cancellationToken);
        return agents.Select(a => a.ToDto()).ToList();
    }

    public async Task<ServiceResult<AgentDetailsDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var agent = await _context.Agents
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (agent == null)
            return ServiceResult<AgentDetailsDto>.NotFound($"Agent with id {id} not found");

        var deals = await _context.Deals
            .AsNoTracking()
            .Where(d => d.RealtorId == id)
            .OrderByDescending(d => d.Date)
            .Select(MappingExtensions.DealToSummary)
            .ToListAsync(cancellationToken);

        return ServiceResult<AgentDetailsDto>.Ok(agent.ToDetailsDto(deals));
    }

    public async Task<ServiceResult<AgentDto>> CreateAsync(CreateAgentDto dto, CancellationToken cancellationToken = default)
    {
        var email = MappingExtensions.NormalizeEmail(dto.Email);
        var license = dto.LicenseNumber.Trim();

        if (await _context.Agents.AnyAsync(a => a.Email.ToLower() == email, cancellationToken))
            return ServiceResult<AgentDto>.Conflict($"Agent with email '{email}' already exists");

        if (await _context.Agents.AnyAsync(a => a.LicenseNumber == license, cancellationToken))
            return ServiceResult<AgentDto>.Conflict($"Agent with license '{license}' already exists");

        var newAgent = dto.ToEntity();

        await _context.Agents.AddAsync(newAgent, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult<AgentDto>.Created(newAgent.ToDto());
    }

    public async Task<ServiceResult> UpdateAsync(int id, UpdateAgentDto dto, CancellationToken cancellationToken = default)
    {
        var agent = await _context.Agents.FindAsync([id], cancellationToken);

        if (agent == null)
            return ServiceResult.NotFound($"Agent with id {id} not found");

        var email = MappingExtensions.NormalizeEmail(dto.Email);
        var license = dto.LicenseNumber.Trim();

        if (await _context.Agents.AnyAsync(a => a.Id != id && a.Email.ToLower() == email, cancellationToken))
            return ServiceResult.Conflict($"Agent with email '{email}' already exists");

        if (await _context.Agents.AnyAsync(a => a.Id != id && a.LicenseNumber == license, cancellationToken))
            return ServiceResult.Conflict($"Agent with license '{license}' already exists");

        agent.UpdateFrom(dto);
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var agent = await _context.Agents.FindAsync([id], cancellationToken);

        if (agent == null)
            return ServiceResult.NotFound($"Agent with id {id} not found");

        if (await _context.Deals.AnyAsync(d => d.RealtorId == id, cancellationToken))
            return ServiceResult.Conflict("Agent has deals and cannot be deleted. Set isActive to false instead.");

        _context.Agents.Remove(agent);
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult.Ok();
    }
}
