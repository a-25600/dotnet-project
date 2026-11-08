using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApi.Data;
using RealEstateApi.DTOs.Agents;
using RealEstateApi.Models;
using RealEstateApi.Services.Common;
using RealEstateApi.Services.Interfaces;

namespace RealEstateApi.Controllers;

/// <summary>
/// Керування ріелторами агентства нерухомості.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgentsController : ControllerBase
{
    private readonly IAgentService _agentService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;

    public AgentsController(
        IAgentService agentService,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context)
    {
        _agentService = agentService;
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Administrator,Agent")]
    [ProducesResponseType(typeof(List<AgentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAgents([FromQuery] AgentQueryParameters parameters, CancellationToken cancellationToken)
    {
        var agents = await _agentService.GetAllAsync(parameters, cancellationToken);
        return Ok(agents);
    }

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

    [HttpPost]
    [ProducesResponseType(typeof(AgentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Roles = AppRoles.Administrator)]
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

        var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email };
        var identityResult = await _userManager.CreateAsync(user, "Agent2026!");

        if (identityResult.Succeeded)
        {
            if (!await _roleManager.RoleExistsAsync("Agent"))
                await _roleManager.CreateAsync(new IdentityRole("Agent"));

            await _userManager.AddToRoleAsync(user, "Agent");

            var agentEntity = await _context.Agents.FindAsync(new object[] { result.Data!.Id }, cancellationToken);
            if (agentEntity != null)
            {
                agentEntity.IdentityUserId = user.Id;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        return CreatedAtAction(nameof(GetAgentById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Roles = AppRoles.Administrator)]
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

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Roles = AppRoles.Administrator)]
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