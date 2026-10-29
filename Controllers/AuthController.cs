using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApi.DTOs.Auth;
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

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Зареєструвати нового користувача системи з роллю User.
    /// </summary>
    /// <param name="dto">Параметри реєстрації: email та пароль (від 8 символів, великі, малі літери та цифри).</param>
    /// <response code="201">Користувача успішно створено, повернуто токен доступу.</response>
    /// <response code="400">Некоректні вхідні дані або пароль не відповідає політиці безпеки.</response>
    /// <response code="409">Користувач із вказаною електронною поштою вже існує.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);

        return result.Status switch
        {
            ServiceStatus.Created => StatusCode(StatusCodes.Status201Created, result.Data),
            ServiceStatus.Conflict => Conflict(new { message = result.ErrorMessage }),
            _ => BadRequest(new { message = result.ErrorMessage })
        };
    }

    /// <summary>
    /// Увійти в систему за допомогою email та пароля й отримати JWT-токен.
    /// </summary>
    /// <param name="dto">Облікові дані для входу.</param>
    /// <response code="200">Успішний вхід, повертається JWT-токен та список ролей.</response>
    /// <response code="401">Невірна електронна пошта або пароль.</response>
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
    /// <response code="200">Дані поточного користувача успішно отримано.</response>
    /// <response code="401">Токен відсутній, недійсний або термін його дії закінчився.</response>
    [HttpGet("me")]
    [Authorize(Roles = AppRoles.UserOrAdministrator)]
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