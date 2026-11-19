using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UsersService.DTOs.Auth;
using UsersService.Models;
using UsersService.Services.Common;
using UsersService.Services.Interfaces;

namespace UsersService.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtSettings _jwt;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IOptions<JwtSettings> jwtOptions,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _jwt = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _userManager.FindByEmailAsync(email) != null)
        {
            _logger.LogWarning("Registration rejected: email {Email} is already registered", email);
            return ServiceResult<AuthResponseDto>.Conflict($"Користувач із електронною поштою '{email}' уже зареєстрований у системі.");
        }

        var user = new ApplicationUser 
        { 
            UserName = email, 
            Email = email 
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Registration failed for {Email}: {Errors}", email, errors);
            return ServiceResult<AuthResponseDto>.BadRequest(errors);
        }

        await _userManager.AddToRoleAsync(user, AppRoles.User);
        _logger.LogInformation("User {UserId} registered with role {Role}", user.Id, AppRoles.User);

        return ServiceResult<AuthResponseDto>.Created(CreateToken(user, new List<string> { AppRoles.User }));
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            _logger.LogWarning("Failed login attempt for {Email}", email);
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        _logger.LogInformation("User {UserId} logged in", user.Id);

        return CreateToken(user, roles);
    }

    private AuthResponseDto CreateToken(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim("role", role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwt.ExpiresMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expires,
            Email = user.Email ?? string.Empty,
            Roles = roles.ToList()
        };
    }
}