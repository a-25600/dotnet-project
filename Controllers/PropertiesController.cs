using Microsoft.AspNetCore.Mvc;
using RealEstateApi.DTOs.Properties;
using RealEstateApi.Services.Common;
using RealEstateApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace RealEstateApi.Controllers;

/// <summary>
/// Керування об'єктами нерухомості агентства.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AppRoles.UserOrAdministrator)]
public class PropertiesController : ControllerBase
{
    private readonly IPropertyService _propertyService;


    public PropertiesController(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    /// <summary>
    /// Отримати список об'єктів нерухомості з фільтрацією, пошуком та сортуванням.
    /// </summary>
    /// <param name="parameters">Параметри фільтрації (ціна, кімнати, тип, доступність), пошук та сортування.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="200">Успішне отримання списку об'єктів нерухомості.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<PropertyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProperties([FromQuery] PropertyQueryParameters parameters, CancellationToken cancellationToken)
    {
        var properties = await _propertyService.GetAllAsync(parameters, cancellationToken);
        return Ok(properties);
    }

    /// <summary>
    /// Отримати детальну інформацію про об'єкт нерухомості за ідентифікатором.
    /// </summary>
    /// <param name="id">Унікальний ідентифікатор об'єкта.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="200">Об'єкт знайдено, повертаються деталі та історія угод.</response>
    /// <response code="404">Об'єкт із вказаним ідентифікатором не знайдено.</response>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PropertyDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPropertyById(int id, CancellationToken cancellationToken)
    {
        var result = await _propertyService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Створити новий об'єкт нерухомості.
    /// </summary>
    /// <param name="dto">Дані для створення об'єкта нерухомості.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="201">Об'єкт успішно створено.</response>
    /// <response code="400">Некоректні вхідні дані.</response>
    [HttpPost]
    [ProducesResponseType(typeof(PropertyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProperty([FromBody] CreatePropertyDto dto, CancellationToken cancellationToken)
    {
        var result = await _propertyService.CreateAsync(dto, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return CreatedAtAction(nameof(GetPropertyById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Оновити дані об'єкта нерухомості.
    /// </summary>
    /// <param name="id">Ідентифікатор об'єкта нерухомості.</param>
    /// <param name="dto">Оновлені дані об'єкта.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="204">Об'єкт успішно оновлено.</response>
    /// <response code="400">Некоректні вхідні дані.</response>
    /// <response code="404">Об'єкт із вказаним ідентифікатором не знайдено.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProperty(int id, [FromBody] UpdatePropertyDto dto, CancellationToken cancellationToken)
    {
        var result = await _propertyService.UpdateAsync(id, dto, cancellationToken);

        return result.Status switch
        {
            ServiceStatus.NotFound => NotFound(new { message = result.ErrorMessage }),
            ServiceStatus.BadRequest => BadRequest(new { message = result.ErrorMessage }),
            _ => NoContent()
        };
    }

    /// <summary>
    /// Видалити об'єкт нерухомості. Об'єкт не можна видалити, якщо до нього прив'язані угоди.
    /// </summary>
    /// <param name="id">Ідентифікатор об'єкта для видалення.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="204">Об'єкт успішно видалено.</response>
    /// <response code="404">Об'єкт із вказаним ідентифікатором не знайдено.</response>
    /// <response code="409">Об'єкт має пов'язані угоди і не може бути видалений.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteProperty(int id, CancellationToken cancellationToken)
    {
        var result = await _propertyService.DeleteAsync(id, cancellationToken);

        return result.Status switch
        {
            ServiceStatus.NotFound => NotFound(new { message = result.ErrorMessage }),
            ServiceStatus.Conflict => Conflict(new { message = result.ErrorMessage }),
            _ => NoContent()
        };
    }
}
