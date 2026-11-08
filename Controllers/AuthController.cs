using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApi.Data;
using RealEstateApi.DTOs.Auth;
using RealEstateApi.Models;
using RealEstateApi.Services.Common;
using RealEstateApi.Services.Interfaces;

namespace RealEstateApi.Controllers;

/// <summary>
/// Автентифікація, реєстрація користувачів та операції з JWT-токенами.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;

    public AuthController(
        IAuthService authService,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context)
    {
        _authService = authService;
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    /// <summary>
    /// Зареєструвати нового користувача системи з роллю Client.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);

        if (result.Status == ServiceStatus.Created)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user != null)
            {
                if (!await _roleManager.RoleExistsAsync("Client"))
                    await _roleManager.CreateAsync(new IdentityRole("Client"));

                if (!await _userManager.IsInRoleAsync(user, "Client"))
                    await _userManager.AddToRoleAsync(user, "Client");

                var client = new Client
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    IdentityUserId = user.Id
                };

                _context.Clients.Add(client);
                await _context.SaveChangesAsync();
            }

            return StatusCode(StatusCodes.Status201Created, result.Data);
        }

        return result.Status switch
        {
            ServiceStatus.Conflict => Conflict(new { message = result.ErrorMessage }),
            _ => BadRequest(new { message = result.ErrorMessage })
        };
    }

    /// <summary>
    /// Увійти в систему за допомогою email та пароля й отримати JWT-токен.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var response = await _authService.LoginAsync(dto);

        if (response == null)
            return Unauthorized(new { message = "Невірна електронна пошта або пароль." });

        return Ok(response);
    }

    /// <summary>
    /// Отримати інформацію про поточного авторизованого користувача з токена.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(CurrentUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        var dto = new CurrentUserDto
        {
            Id = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? string.Empty,
            Email = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value ?? string.Empty,
            Roles = User.FindAll("role").Select(c => c.Value).ToList()
        };

        return Ok(dto);
    }
}