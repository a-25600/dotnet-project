namespace RealEstateApi.Services.Common;

/// <summary>
/// Параметри конфігурації генерації та валідації JWT-токенів із файлу appsettings.json
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Видавець токена (Issuer)
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Одержувач токена (Audience)
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Секретний ключ для підпису токена (мінімум 32 символи)
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Термін дії токена доступу у хвилинах
    /// </summary>
    public int ExpiresMinutes { get; set; } = 60;
}