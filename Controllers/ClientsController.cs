using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApi.Data;
using RealEstateApi.DTOs.Clients;
using RealEstateApi.Mappings;

namespace RealEstateApi.Controllers;

/// <summary>
/// Керування клієнтами агентства.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ClientsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Список клієнтів із пошуком і сортуванням.
    /// </summary>
    /// <param name="parameters">search - ПІБ, email або телефон; sortBy - id, email, phone, name (за замовчуванням); sortOrder - asc або desc.</param>
    /// <response code="200">Список клієнтів.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<ClientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllClients([FromQuery] ClientQueryParameters parameters)
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

        var clients = await query.ToListAsync();
        return Ok(clients.Select(c => c.ToDto()).ToList());
    }

    /// <summary>
    /// Клієнт за ідентифікатором разом зі списком його угод.
    /// </summary>
    /// <param name="id">Ідентифікатор клієнта.</param>
    /// <response code="200">Клієнт із вкладеними угодами.</response>
    /// <response code="404">Клієнта не знайдено.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClientDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClientById(int id)
    {
        var client = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
            return NotFound(new { message = $"Client with id {id} not found" });

        var deals = await _context.Deals
            .AsNoTracking()
            .Where(d => d.ClientId == id)
            .OrderByDescending(d => d.Date)
            .Select(MappingExtensions.DealToSummary)
            .ToListAsync();

        return Ok(client.ToDetailsDto(deals));
    }

    /// <summary>
    /// Створити клієнта.
    /// </summary>
    /// <response code="201">Клієнта створено.</response>
    /// <response code="400">Некоректні вхідні дані.</response>
    /// <response code="409">Клієнт з таким email уже існує.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateClient([FromBody] CreateClientDto dto)
    {
        var email = MappingExtensions.NormalizeEmail(dto.Email);

        if (await _context.Clients.AnyAsync(c => c.Email.ToLower() == email))
            return Conflict(new { message = $"Client with email {email} already exists" });

        var newClient = dto.ToEntity();

        _context.Clients.Add(newClient);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetClientById), new { id = newClient.Id }, newClient.ToDto());
    }

    /// <summary>
    /// Оновити дані клієнта.
    /// </summary>
    /// <param name="id">Ідентифікатор клієнта.</param>
    /// <response code="204">Клієнта оновлено.</response>
    /// <response code="400">Некоректні вхідні дані.</response>
    /// <response code="404">Клієнта не знайдено.</response>
    /// <response code="409">Інший клієнт уже має такий email.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateClient(int id, [FromBody] UpdateClientDto dto)
    {
        var client = await _context.Clients.FindAsync(id);

        if (client == null)
            return NotFound(new { message = $"Client with id {id} not found" });

        var email = MappingExtensions.NormalizeEmail(dto.Email);

        if (await _context.Clients.AnyAsync(c => c.Id != id && c.Email.ToLower() == email))
            return Conflict(new { message = $"Client with email {email} already exists" });

        client.UpdateFrom(dto);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Видалити клієнта. Клієнта, який має угоди, видалити не можна.
    /// </summary>
    /// <param name="id">Ідентифікатор клієнта.</param>
    /// <response code="204">Клієнта видалено.</response>
    /// <response code="404">Клієнта не знайдено.</response>
    /// <response code="409">У клієнта є угоди.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteClient(int id)
    {
        var client = await _context.Clients.FindAsync(id);

        if (client == null)
            return NotFound(new { message = $"Client with id {id} not found" });

        if (await _context.Deals.AnyAsync(d => d.ClientId == id))
            return Conflict(new { message = "Client has deals and cannot be deleted" });

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}