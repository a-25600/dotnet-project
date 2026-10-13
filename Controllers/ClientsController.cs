using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApi.Data;
using RealEstateApi.DTOs.Clients;
using RealEstateApi.Models;

namespace RealEstateApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ClientsController(ApplicationDbContext context)
    {
        _context = context;
    }

    private static ClientDto ToDto(Client client) => new()
    {
        Id = client.Id,
        FullName = client.FullName,
        Email = client.Email,
        PhoneNumber = client.PhoneNumber
    };

    [HttpGet]
    public async Task<IActionResult> GetAllClients([FromQuery] ClientQueryParameters parameters)
    {
        var query = _context.Clients.AsQueryable();

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

        var clients = await query.ToListAsync();
        return Ok(clients.Select(ToDto).ToList());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetClientById(int id)
    {
        var client = await _context.Clients.FindAsync(id);

        if (client == null)
            return NotFound(new { message = $"Client with id {id} not found" });

        return Ok(ToDto(client));
    }

    [HttpPost]
    public async Task<IActionResult> CreateClient([FromBody] CreateClientDto dto)
    {
        var newClient = new Client
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber
        };

        _context.Clients.Add(newClient);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetClientById), new { id = newClient.Id }, ToDto(newClient));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateClient(int id, [FromBody] UpdateClientDto dto)
    {
        var client = await _context.Clients.FindAsync(id);

        if (client == null)
            return NotFound();

        client.FullName = dto.FullName;
        client.Email = dto.Email;
        client.PhoneNumber = dto.PhoneNumber;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClient(int id)
    {
        var client = await _context.Clients.FindAsync(id);

        if (client == null)
            return NotFound();

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
