namespace RealEstateApi.DTOs.Auth;

/// <summary>
/// Інформація про поточного авторизованого користувача.
/// </summary>
public class CurrentUserDto
{
    /// <summary>
    /// Унікальний ідентифікатор користувача.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Електронна пошта користувача.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Список призначених ролей.
    /// </summary>
    public List<string> Roles { get; set; } = new();
}