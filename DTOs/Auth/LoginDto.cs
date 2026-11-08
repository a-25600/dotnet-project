using System.ComponentModel.DataAnnotations;

namespace RealEstateApi.DTOs.Auth;

/// <summary>
/// Дані для входу в систему.
/// </summary>
public class LoginDto
{
    /// <summary>
    /// Електронна пошта користувача.
    /// </summary>
    [Required(ErrorMessage = "Поле Email є обов'язковим для заповнення")]
    [EmailAddress(ErrorMessage = "Введено некоректний формат електронної пошти")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Пароль користувача.
    /// </summary>
    [Required(ErrorMessage = "Поле Пароль є обов'язковим для заповнення")]
    public string Password { get; set; } = string.Empty;
}