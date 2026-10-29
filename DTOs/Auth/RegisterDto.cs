using System.ComponentModel.DataAnnotations;

namespace RealEstateApi.DTOs.Auth;

/// <summary>
/// Дані для реєстрації нового користувача.
/// </summary>
public class RegisterDto
{
    /// <summary>
    /// Електронна пошта користувача.
    /// </summary>
    [Required(ErrorMessage = "Поле Email є обов'язковим для заповнення")]
    [StringLength(100, ErrorMessage = "Email не повинен перевищувати 100 символів")]
    [EmailAddress(ErrorMessage = "Введено некоректний формат електронної пошти")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Пароль користувача (мінімум 8 символів).
    /// </summary>
    [Required(ErrorMessage = "Поле Пароль є обов'язковим для заповнення")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Пароль повинен містити від 8 до 100 символів")]
    public string Password { get; set; } = string.Empty;
}