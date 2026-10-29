using Microsoft.AspNetCore.Mvc;
using RealEstateApi.DTOs.Deals;
using RealEstateApi.Services.Common;
using RealEstateApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
namespace RealEstateApi.Controllers;

/// <summary>
/// Керування угодами агентства нерухомості.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DealsController : ControllerBase
{
    private readonly IDealService _dealService;

    public DealsController(IDealService dealService)
    {
        _dealService = dealService;
    }

    /// <summary>
    /// Отримати список угод із можливістю фільтрації та сортування.
    /// </summary>
    /// <param name="parameters">Параметри фільтрації (статус, об'єкт, клієнт, ріелтор, сума) та сортування.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="200">Список угод.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<DealDto>), StatusCodes.Status200OK)]
    [Authorize(Roles = AppRoles.UserOrAdministrator)]
    public async Task<IActionResult> GetAllDeals([FromQuery] DealQueryParameters parameters, CancellationToken cancellationToken)
    {
        var deals = await _dealService.GetAllAsync(parameters, cancellationToken);
        return Ok(deals);
    }

    /// <summary>
    /// Отримати угоду за її ідентифікатором.
    /// </summary>
    /// <param name="id">Унікальний ідентифікатор угоди.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="200">Угоду знайдено.</response>
    /// <response code="404">Угоду з вказаним ідентифікатором не знайдено.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DealDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDealById(int id, CancellationToken cancellationToken)
    {
        var result = await _dealService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Створити нову угоду в системі.
    /// </summary>
    /// <param name="dto">Дані для створення угоди.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="201">Угоду успішно створено.</response>
    /// <response code="400">Некоректні вхідні дані або відсутність пов'язаних сутностей / недоступність об'єкта чи ріелтора.</response>
    [HttpPost]
    [ProducesResponseType(typeof(DealDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDeal([FromBody] CreateDealDto dto, CancellationToken cancellationToken)
    {
        var result = await _dealService.CreateAsync(dto, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return CreatedAtAction(nameof(GetDealById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Оновити параметри угоди (суму або статус).
    /// </summary>
    /// <param name="id">Ідентифікатор угоди.</param>
    /// <param name="dto">Оновлені дані угоди (сума та новий статус).</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="204">Угоду успішно оновлено.</response>
    /// <response code="400">Некоректні вхідні дані або неприпустимий статус.</response>
    /// <response code="404">Угоду з вказаним ідентифікатором не знайдено.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDeal(int id, [FromBody] UpdateDealDto dto, CancellationToken cancellationToken)
    {
        var result = await _dealService.UpdateAsync(id, dto, cancellationToken);

        return result.Status switch
        {
            ServiceStatus.NotFound => NotFound(new { message = result.ErrorMessage }),
            ServiceStatus.BadRequest => BadRequest(new { message = result.ErrorMessage }),
            _ => NoContent()
        };
    }

    /// <summary>
    /// Видалити угоду за ідентифікатором.
    /// </summary>
    /// <param name="id">Ідентифікатор угоди для видалення.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="204">Угоду успішно видалено.</response>
    /// <response code="404">Угоду з вказаним ідентифікатором не знайдено.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> DeleteDeal(int id, CancellationToken cancellationToken)
    {
        var result = await _dealService.DeleteAsync(id, cancellationToken);

        return result.Status switch
        {
            ServiceStatus.NotFound => NotFound(new { message = result.ErrorMessage }),
            _ => NoContent()
        };
    }
}
