using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApi.Data;
using RealEstateApi.DTOs.Properties;
using RealEstateApi.Mappings;

namespace RealEstateApi.Controllers;

/// <summary>
/// Керування об'єктами нерухомості агентства.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PropertiesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PropertiesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Отримати список об'єктів нерухомості з фільтрацією, пошуком та сортуванням.
    /// </summary>
    /// <param name="parameters">Параметри фільтрації (ціна, кімнати, тип, доступність), пошук та сортування.</param>
    /// <response code="200">Успішне отримання списку об'єктів нерухомості.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<PropertyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProperties([FromQuery] PropertyQueryParameters parameters)
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

        var properties = await query.ToListAsync();
        return Ok(properties.Select(p => p.ToDto()).ToList());
    }

    /// <summary>
    /// Отримати детальну інформацію про об'єкт нерухомості за ідентифікатором.
    /// </summary>
    /// <param name="id">Унікальний ідентифікатор об'єкта.</param>
    /// <response code="200">Об'єкт знайдено, повертаються деталі та історія угод.</response>
    /// <response code="404">Об'єкт із вказаним ідентифікатором не знайдено.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PropertyDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPropertyById(int id)
    {
        var property = await _context.Properties.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

        if (property == null)
            return NotFound(new { message = $"Property with id {id} not found" });

        var deals = await _context.Deals
            .AsNoTracking()
            .Where(d => d.PropertyId == id)
            .OrderByDescending(d => d.Date)
            .Select(MappingExtensions.DealToSummary)
            .ToListAsync();

        return Ok(property.ToDetailsDto(deals));
    }

    /// <summary>
    /// Створити новий об'єкт нерухомості.
    /// </summary>
    /// <param name="dto">Дані для створення об'єкта нерухомості.</param>
    /// <response code="201">Об'єкт успішно створено.</response>
    /// <response code="400">Некоректні вхідні дані.</response>
    [HttpPost]
    [ProducesResponseType(typeof(PropertyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProperty([FromBody] CreatePropertyDto dto)
    {
        var newProperty = dto.ToEntity();

        await _context.Properties.AddAsync(newProperty);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPropertyById), new { id = newProperty.Id }, newProperty.ToDto());
    }

    /// <summary>
    /// Оновити дані об'єкта нерухомості.
    /// </summary>
    /// <param name="id">Ідентифікатор об'єкта нерухомості.</param>
    /// <param name="dto">Оновлені дані об'єкта.</param>
    /// <response code="204">Об'єкт успішно оновлено.</response>
    /// <response code="400">Некоректні вхідні дані.</response>
    /// <response code="404">Об'єкт із вказаним ідентифікатором не знайдено.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProperty(int id, [FromBody] UpdatePropertyDto dto)
    {
        var existingProperty = await _context.Properties.FindAsync(id);

        if (existingProperty == null)
            return NotFound(new { message = $"Property with id {id} not found" });

        existingProperty.UpdateFrom(dto);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Видалити об'єкт нерухомості. Об'єкт не можна видалити, якщо до нього прив'язані угоди.
    /// </summary>
    /// <param name="id">Ідентифікатор об'єкта для видалення.</param>
    /// <response code="204">Об'єкт успішно видалено.</response>
    /// <response code="404">Об'єкт із вказаним ідентифікатором не знайдено.</response>
    /// <response code="409">Об'єкт має пов'язані угоди і не може бути видалений.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteProperty(int id)
    {
        var property = await _context.Properties.FindAsync(id);

        if (property == null)
            return NotFound(new { message = $"Property with id {id} not found" });

        if (await _context.Deals.AnyAsync(d => d.PropertyId == id))
            return Conflict(new { message = "Property has associated deals and cannot be deleted. Change availability status instead." });

        _context.Properties.Remove(property);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}