using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApi.Data;
using RealEstateApi.DTOs.Agents;
using RealEstateApi.Mappings;

namespace RealEstateApi.Controllers;

/// <summary>
/// Керування ріелторами агентства нерухомості.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AgentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AgentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Отримати список ріелторів із фільтром за активністю, пошуком і сортуванням.
    /// </summary>
    /// <param name="parameters">Параметри запиту: isActive, search (ПІБ, email, телефон, ліцензія), sortBy, sortOrder.</param>
    /// <response code="200">Список ріелторів.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<AgentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAgents([FromQuery] AgentQueryParameters parameters)
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

        var agents = await query.ToListAsync();
        return Ok(agents.Select(a => a.ToDto()).ToList());
    }

    /// <summary>
    /// Отримати дані ріелтора за ідентифікатором разом зі списком його угод.
    /// </summary>
    /// <param name="id">Ідентифікатор ріелтора.</param>
    /// <response code="200">Ріелтор із вкладеними угодами.</response>
    /// <response code="404">Ріелтора не знайдено.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AgentDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAgentById(int id)
    {
        var agent = await _context.Agents.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);

        if (agent == null)
            return NotFound(new { message = $"Agent with id {id} not found" });

        var deals = await _context.Deals
            .AsNoTracking()
            .Where(d => d.RealtorId == id)
            .OrderByDescending(d => d.Date)
            .Select(MappingExtensions.DealToSummary)
            .ToListAsync();

        return Ok(agent.ToDetailsDto(deals));
    }

    /// <summary>
    /// Зареєструвати нового ріелтора.
    /// </summary>
    /// <param name="dto">Дані для створення ріелтора.</param>
    /// <response code="201">Ріелтора успішно створено.</response>
    /// <response code="400">Некоректні вхідні дані.</response>
    /// <response code="409">Ріелтор із таким email або номером ліцензії вже існує.</response>
    [HttpPost]
    [ProducesResponseType(typeof(AgentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAgent([FromBody] CreateAgentDto dto)
    {
        var email = MappingExtensions.NormalizeEmail(dto.Email);
        var license = dto.LicenseNumber.Trim();

        if (await _context.Agents.AnyAsync(a => a.Email.ToLower() == email))
            return Conflict(new { message = $"Agent with email '{email}' already exists" });

        if (await _context.Agents.AnyAsync(a => a.LicenseNumber == license))
            return Conflict(new { message = $"Agent with license '{license}' already exists" });

        var newAgent = dto.ToEntity();

        await _context.Agents.AddAsync(newAgent);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAgentById), new { id = newAgent.Id }, newAgent.ToDto());
    }

    /// <summary>
    /// Оновити дані ріелтора.
    /// </summary>
    /// <param name="id">Ідентифікатор ріелтора.</param>
    /// <param name="dto">Оновлені дані ріелтора.</param>
    /// <response code="204">Ріелтора успішно оновлено.</response>
    /// <response code="400">Некоректні вхідні дані.</response>
    /// <response code="404">Ріелтора не знайдено.</response>
    /// <response code="409">Інший ріелтор уже має такий email або номер ліцензії.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAgent(int id, [FromBody] UpdateAgentDto dto)
    {
        var agent = await _context.Agents.FindAsync(id);

        if (agent == null)
            return NotFound(new { message = $"Agent with id {id} not found" });

        var email = MappingExtensions.NormalizeEmail(dto.Email);
        var license = dto.LicenseNumber.Trim();

        if (await _context.Agents.AnyAsync(a => a.Id != id && a.Email.ToLower() == email))
            return Conflict(new { message = $"Agent with email '{email}' already exists" });

        if (await _context.Agents.AnyAsync(a => a.Id != id && a.LicenseNumber == license))
            return Conflict(new { message = $"Agent with license '{license}' already exists" });

        agent.UpdateFrom(dto);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Видалити ріелтора. Ріелтора, який має укладені угоди, видалити не можна.
    /// </summary>
    /// <param name="id">Ідентифікатор ріелтора.</param>
    /// <response code="204">Ріелтора успішно видалено.</response>
    /// <response code="404">Ріелтора не знайдено.</response>
    /// <response code="409">Ріелтор має прив'язані угоди. Рекомендується деактивувати (isActive = false).</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAgent(int id)
    {
        var agent = await _context.Agents.FindAsync(id);

        if (agent == null)
            return NotFound(new { message = $"Agent with id {id} not found" });

        if (await _context.Deals.AnyAsync(d => d.RealtorId == id))
            return Conflict(new { message = "Agent has deals and cannot be deleted. Set isActive to false instead." });

        _context.Agents.Remove(agent);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}