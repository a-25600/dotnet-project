using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApi.Data;
using RealEstateApi.DTOs.Agents;
using RealEstateApi.Models;

namespace RealEstateApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AgentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    private static AgentDto ToDto(Agent agent) => new()
    {
        Id = agent.Id,
        FullName = agent.FullName,
        Email = agent.Email,
        PhoneNumber = agent.PhoneNumber,
        LicenseNumber = agent.LicenseNumber,
        IsActive = agent.IsActive
    };

    [HttpGet]
    public async Task<IActionResult> GetAgents([FromQuery] AgentQueryParameters parameters)
    {
        var query = _context.Agents.AsQueryable();

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

        var agents = await query.ToListAsync();
        return Ok(agents.Select(ToDto).ToList());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAgentById(int id)
    {
        var agent = await _context.Agents.FindAsync(id);

        if (agent == null)
            return NotFound(new { message = $"Agent with id {id} not found" });

        return Ok(ToDto(agent));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAgent([FromBody] CreateAgentDto dto)
    {
        var newAgent = new Agent
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            LicenseNumber = dto.LicenseNumber,
            IsActive = true
        };

        _context.Agents.Add(newAgent);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAgentById), new { id = newAgent.Id }, ToDto(newAgent));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAgent(int id, [FromBody] UpdateAgentDto dto)
    {
        var agent = await _context.Agents.FindAsync(id);

        if (agent == null)
            return NotFound();

        agent.FullName = dto.FullName;
        agent.Email = dto.Email;
        agent.PhoneNumber = dto.PhoneNumber;
        agent.LicenseNumber = dto.LicenseNumber;
        agent.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAgent(int id)
    {
        var agent = await _context.Agents.FindAsync(id);

        if (agent == null)
            return NotFound();

        _context.Agents.Remove(agent);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
