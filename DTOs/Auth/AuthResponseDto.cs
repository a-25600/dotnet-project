namespace RealEstateApi.DTOs.Auth;

/// <summary>
/// Відповідь сервісу автентифікації з токеном доступу.
/// </summary>
public class AuthResponseDto
{
    /// <summary>
    /// Згенерований JWT-токен доступу.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Дата та час завершення терміну дії токена (UTC).
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Електронна пошта автентифікованого користувача.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Перелік ролей, призначених користувачу.
    /// </summary>
    public List<string> Roles { get; set; } = new();
}