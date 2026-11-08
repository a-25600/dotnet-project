using RealEstateApi.DTOs.Auth;
using RealEstateApi.Services.Common;

namespace RealEstateApi.Services.Interfaces;

/// <summary>
/// Сервіс автентифікації та реєстрації користувачів у системі
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Зареєструвати нового користувача з роллю за замовчуванням та згенерувати токен
    /// </summary>
    /// <param name="dto">Дані для реєстрації облікового запису</param>
    /// <returns>Результат операції з даними користувача та токеном доступу</returns>
    Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto dto);

    /// <summary>
    /// Виконати перевірку облікових даних користувача та сформувати токен доступу
    /// </summary>
    /// <param name="dto">Дані облікового запису для входу</param>
    /// <returns>Дані з токеном або null, якщо дані невірні</returns>
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
}