using Microsoft.AspNetCore.Mvc;
using RealEstateApi.DTOs.Agents;
using RealEstateApi.Services.Common;
using RealEstateApi.Services.Interfaces;

namespace RealEstateApi.Controllers;

/// <summary>
/// Керування ріелторами агентства нерухомості.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AgentsController : ControllerBase
{
    private readonly IAgentService _agentService;

    public AgentsController(IAgentService agentService)
    {
        _agentService = agentService;
    }

    /// <summary>
    /// Отримати список ріелторів із фільтром за активністю, пошуком і сортуванням.
    /// </summary>
    /// <param name="parameters">Параметри запиту: isActive, search (ПІБ, email, телефон, ліцензія), sortBy, sortOrder.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="200">Список ріелторів.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<AgentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAgents([FromQuery] AgentQueryParameters parameters, CancellationToken cancellationToken)
    {
        var agents = await _agentService.GetAllAsync(parameters, cancellationToken);
        return Ok(agents);
    }

    /// <summary>
    /// Отримати дані ріелтора за ідентифікатором разом зі списком його угод.
    /// </summary>
    /// <param name="id">Ідентифікатор ріелтора.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="200">Ріелтор із вкладеними угодами.</response>
    /// <response code="404">Ріелтора не знайдено.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AgentDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAgentById(int id, CancellationToken cancellationToken)
    {
        var result = await _agentService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Зареєструвати нового ріелтора.
    /// </summary>
    /// <param name="dto">Дані для створення ріелтора.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="201">Ріелтора успішно створено.</response>
    /// <response code="400">Некоректні вхідні дані.</response>
    /// <response code="409">Ріелтор із таким email або номером ліцензії вже існує.</response>
    [HttpPost]
    [ProducesResponseType(typeof(AgentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAgent([FromBody] CreateAgentDto dto, CancellationToken cancellationToken)
    {
        var result = await _agentService.CreateAsync(dto, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Status switch
            {
                ServiceStatus.Conflict => Conflict(new { message = result.ErrorMessage }),
                _ => BadRequest(new { message = result.ErrorMessage })
            };
        }

        return CreatedAtAction(nameof(GetAgentById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Оновити дані ріелтора.
    /// </summary>
    /// <param name="id">Ідентифікатор ріелтора.</param>
    /// <param name="dto">Оновлені дані ріелтора.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="204">Ріелтора успішно оновлено.</response>
    /// <response code="400">Некоректні вхідні дані.</response>
    /// <response code="404">Ріелтора не знайдено.</response>
    /// <response code="409">Інший ріелтор уже має такий email або номер ліцензії.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAgent(int id, [FromBody] UpdateAgentDto dto, CancellationToken cancellationToken)
    {
        var result = await _agentService.UpdateAsync(id, dto, cancellationToken);

        return result.Status switch
        {
            ServiceStatus.NotFound => NotFound(new { message = result.ErrorMessage }),
            ServiceStatus.Conflict => Conflict(new { message = result.ErrorMessage }),
            ServiceStatus.BadRequest => BadRequest(new { message = result.ErrorMessage }),
            _ => NoContent()
        };
    }

    /// <summary>
    /// Видалити ріелтора. Ріелтора, який має укладені угоди, видалити не можна.
    /// </summary>
    /// <param name="id">Ідентифікатор ріелтора.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="204">Ріелтора успішно видалено.</response>
    /// <response code="404">Ріелтора не знайдено.</response>
    /// <response code="409">Ріелтор має прив'язані угоди. Рекомендується деактивувати (isActive = false).</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAgent(int id, CancellationToken cancellationToken)
    {
        var result = await _agentService.DeleteAsync(id, cancellationToken);

        return result.Status switch
        {
            ServiceStatus.NotFound => NotFound(new { message = result.ErrorMessage }),
            ServiceStatus.Conflict => Conflict(new { message = result.ErrorMessage }),
            _ => NoContent()
        };
    }
}
