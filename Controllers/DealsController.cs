using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApi.Data;
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
    private readonly ApplicationDbContext _context;

    public DealsController(IDealService dealService, ApplicationDbContext context)
    {
        _dealService = dealService;
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<DealDto>), StatusCodes.Status200OK)]
    [Authorize]
    public async Task<IActionResult> GetAllDeals([FromQuery] DealQueryParameters parameters, CancellationToken cancellationToken)
    {
        var deals = await _dealService.GetAllAsync(parameters, cancellationToken);
        return Ok(deals);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DealDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<IActionResult> GetDealById(int id, CancellationToken cancellationToken)
    {
        var result = await _dealService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpPost]
    [ProducesResponseType(typeof(DealDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> CreateDeal([FromBody] CreateDealDto dto, CancellationToken cancellationToken)
    {
        if (User.IsInRole("Client") || User.IsInRole(AppRoles.User))
        {
            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.IdentityUserId == identityUserId, cancellationToken);

            if (client == null)
                return BadRequest(new { message = "Профіль клієнта не знайдено." });

            dto.ClientId = client.Id;
            dto.RealtorId = null;
        }

        var result = await _dealService.CreateAsync(dto, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return CreatedAtAction(nameof(GetDealById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Administrator,Agent")]
    public async Task<IActionResult> UpdateDeal(int id, [FromBody] UpdateDealDto dto, CancellationToken cancellationToken)
    {
        if (User.IsInRole("Agent") && !User.IsInRole("Administrator"))
        {
            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var agent = await _context.Agents.FirstOrDefaultAsync(a => a.IdentityUserId == identityUserId, cancellationToken);

            if (agent == null) return Forbid();

            var deal = await _context.Deals.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
            if (deal == null) return NotFound(new { message = $"Deal with id {id} not found" });

            if (deal.RealtorId != null && deal.RealtorId != agent.Id)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Ви можете редагувати лише власні або вільні заявки." });
            }

            if (dto.RealtorId != null && dto.RealtorId != agent.Id)
            {
                return BadRequest(new { message = "Ви можете призначити лише себе на цю угоду." });
            }
        }

        var result = await _dealService.UpdateAsync(id, dto, cancellationToken);

        return result.Status switch
        {
            ServiceStatus.NotFound => NotFound(new { message = result.ErrorMessage }),
            ServiceStatus.BadRequest => BadRequest(new { message = result.ErrorMessage }),
            _ => NoContent()
        };
    }

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