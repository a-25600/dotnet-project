using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApi.Data;
using RealEstateApi.DTOs.Deals;
using RealEstateApi.Mappings;
using RealEstateApi.Models;

namespace RealEstateApi.Controllers;

/// <summary>
/// Керування угодами агентства нерухомості.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DealsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DealsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Отримати список угод із можливістю фільтрації та сортування.
    /// </summary>
    /// <param name="parameters">Параметри фільтрації (статус, об'єкт, клієнт, ріелтор, сума) та сортування.</param>
    /// <response code="200">Список угод.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<DealDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllDeals([FromQuery] DealQueryParameters parameters)
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

        var deals = await query.Select(MappingExtensions.DealToDtoExpr).ToListAsync();
        return Ok(deals);
    }

    /// <summary>
    /// Отримати угоду за її ідентифікатором.
    /// </summary>
    /// <param name="id">Унікальний ідентифікатор угоди.</param>
    /// <response code="200">Угоду знайдено.</response>
    /// <response code="404">Угоду з вказаним ідентифікатором не знайдено.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DealDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDealById(int id)
    {
        var deal = await _context.Deals
            .AsNoTracking()
            .Include(d => d.Property)
            .Include(d => d.Client)
            .Include(d => d.Realtor)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (deal == null)
            return NotFound(new { message = $"Deal with id {id} not found" });

        return Ok(deal.ToDto());
    }

    /// <summary>
    /// Створити нову угоду в системі.
    /// </summary>
    /// <param name="dto">Дані для створення угоди.</param>
    /// <response code="201">Угоду успішно створено.</response>
    /// <response code="400">Некоректні вхідні дані або відсутність пов'язаних сутностей / недоступність об'єкта чи ріелтора.</response>
    [HttpPost]
    [ProducesResponseType(typeof(DealDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDeal([FromBody] CreateDealDto dto)
    {
        var property = await _context.Properties.FindAsync(dto.PropertyId);
        if (property == null)
            return BadRequest(new { message = $"Property with id {dto.PropertyId} does not exist" });

        if (!property.IsAvailable)
            return BadRequest(new { message = $"Property with id {dto.PropertyId} is currently not available" });

        var client = await _context.Clients.FindAsync(dto.ClientId);
        if (client == null)
            return BadRequest(new { message = $"Client with id {dto.ClientId} does not exist" });

        var realtor = await _context.Agents.FindAsync(dto.RealtorId);
        if (realtor == null)
            return BadRequest(new { message = $"Agent with id {dto.RealtorId} does not exist" });

        if (!realtor.IsActive)
            return BadRequest(new { message = $"Agent with id {dto.RealtorId} is inactive" });

        var newDeal = new Deal
        {
            PropertyId = dto.PropertyId,
            ClientId = dto.ClientId,
            RealtorId = dto.RealtorId,
            Price = dto.Price,
            Date = DateTime.UtcNow,
            Status = "Pending"
        };

        await _context.Deals.AddAsync(newDeal);
        await _context.SaveChangesAsync();

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

        return CreatedAtAction(nameof(GetDealById), new { id = newDeal.Id }, resultDto);
    }

    /// <summary>
    /// Оновити параметри угоди (суму або статус).
    /// </summary>
    /// <param name="id">Ідентифікатор угоди.</param>
    /// <param name="dto">Оновлені дані угоди (сума та новий статус).</param>
    /// <response code="204">Угоду успішно оновлено.</response>
    /// <response code="400">Некоректні вхідні дані або неприпустимий статус.</response>
    /// <response code="404">Угоду з вказаним ідентифікатором не знайдено.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDeal(int id, [FromBody] UpdateDealDto dto)
    {
        var deal = await _context.Deals.Include(d => d.Property).FirstOrDefaultAsync(d => d.Id == id);

        if (deal == null)
            return NotFound(new { message = $"Deal with id {id} not found" });

        var normalizedStatus = dto.Status.Trim();
        var allowedStatuses = new[] { "Pending", "Completed", "Cancelled" };
        var matchedStatus = allowedStatuses.FirstOrDefault(s => string.Equals(s, normalizedStatus, StringComparison.OrdinalIgnoreCase));

        if (matchedStatus == null)
            return BadRequest(new { message = $"Status must be one of: {string.Join(", ", allowedStatuses)}" });

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

        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Видалити угоду за ідентифікатором.
    /// </summary>
    /// <param name="id">Ідентифікатор угоди для видалення.</param>
    /// <response code="204">Угоду успішно видалено.</response>
    /// <response code="404">Угоду з вказаним ідентифікатором не знайдено.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDeal(int id)
    {
        var deal = await _context.Deals.Include(d => d.Property).FirstOrDefaultAsync(d => d.Id == id);

        if (deal == null)
            return NotFound(new { message = $"Deal with id {id} not found" });

        if (deal.Status == "Completed" && deal.Property != null)
        {
            var otherCompletedDeals = await _context.Deals
                .AnyAsync(d => d.PropertyId == deal.PropertyId && d.Id != id && d.Status == "Completed");

            if (!otherCompletedDeals)
            {
                deal.Property.IsAvailable = true;
            }
        }

        _context.Deals.Remove(deal);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}