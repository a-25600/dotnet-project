using Microsoft.AspNetCore.Mvc;
using RealEstateApi.DTOs.Clients;
using RealEstateApi.Services.Common;
using RealEstateApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
namespace RealEstateApi.Controllers;

/// <summary>
/// Керування клієнтами агентства нерухомості.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AppRoles.UserOrAdministrator)]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    /// <summary>
    /// Отримати список клієнтів із пошуком і сортуванням.
    /// </summary>
    /// <param name="parameters">Параметри запиту: search (ПІБ, email, телефон), sortBy (name, email, phone, id), sortOrder (asc, desc).</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="200">Список клієнтів.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<ClientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllClients([FromQuery] ClientQueryParameters parameters, CancellationToken cancellationToken)
    {
        var clients = await _clientService.GetAllAsync(parameters, cancellationToken);
        return Ok(clients);
    }

    /// <summary>
    /// Отримати клієнта за ідентифікатором разом зі списком його угод.
    /// </summary>
    /// <param name="id">Ідентифікатор клієнта.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="200">Клієнт із вкладеними угодами.</response>
    /// <response code="404">Клієнта не знайдено.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ClientDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClientById(int id, CancellationToken cancellationToken)
    {
        var result = await _clientService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Створити нового клієнта.
    /// </summary>
    /// <param name="dto">Дані для реєстрації клієнта.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="201">Клієнта створено.</response>
    /// <response code="400">Некоректні вхідні дані.</response>
    /// <response code="409">Клієнт з таким email уже існує.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateClient([FromBody] CreateClientDto dto, CancellationToken cancellationToken)
    {
        var result = await _clientService.CreateAsync(dto, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Status switch
            {
                ServiceStatus.Conflict => Conflict(new { message = result.ErrorMessage }),
                _ => BadRequest(new { message = result.ErrorMessage })
            };
        }

        return CreatedAtAction(nameof(GetClientById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Оновити дані клієнта.
    /// </summary>
    /// <param name="id">Ідентифікатор клієнта.</param>
    /// <param name="dto">Оновлені дані клієнта.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="204">Клієнта оновлено.</response>
    /// <response code="400">Некоректні вхідні дані.</response>
    /// <response code="404">Клієнта не знайдено.</response>
    /// <response code="409">Інший клієнт уже має такий email.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateClient(int id, [FromBody] UpdateClientDto dto, CancellationToken cancellationToken)
    {
        var result = await _clientService.UpdateAsync(id, dto, cancellationToken);

        return result.Status switch
        {
            ServiceStatus.NotFound => NotFound(new { message = result.ErrorMessage }),
            ServiceStatus.Conflict => Conflict(new { message = result.ErrorMessage }),
            ServiceStatus.BadRequest => BadRequest(new { message = result.ErrorMessage }),
            _ => NoContent()
        };
    }

    /// <summary>
    /// Видалити клієнта. Клієнта, який має угоди, видалити не можна.
    /// </summary>
    /// <param name="id">Ідентифікатор клієнта.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="204">Клієнта видалено.</response>
    /// <response code="404">Клієнта не знайдено.</response>
    /// <response code="409">У клієнта є угоди.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> DeleteClient(int id, CancellationToken cancellationToken)
    {
        var result = await _clientService.DeleteAsync(id, cancellationToken);

        return result.Status switch
        {
            ServiceStatus.NotFound => NotFound(new { message = result.ErrorMessage }),
            ServiceStatus.Conflict => Conflict(new { message = result.ErrorMessage }),
            _ => NoContent()
        };
    }
}
