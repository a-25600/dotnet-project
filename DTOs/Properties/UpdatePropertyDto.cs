using System.ComponentModel.DataAnnotations;

namespace RealEstateApi.DTOs.Properties;

/// <summary>
/// Дані для повного оновлення об'єкта нерухомості.
/// </summary>
public class UpdatePropertyDto
{
    /// <summary>
    /// Адреса об'єкта (від 5 до 150 символів).
    /// </summary>
    [Required(ErrorMessage = "Адреса є обов'язковою.")]
    [StringLength(150, MinimumLength = 5, ErrorMessage = "Адреса має містити від 5 до 150 символів.")]
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Ціна об'єкта (від 100 до 10 000 000).
    /// </summary>
    [Required(ErrorMessage = "Ціна є обов'язковою.")]
    [Range(100, 10_000_000, ErrorMessage = "Ціна має бути в діапазоні від 100 до 10 000 000.")]
    public decimal Price { get; set; }

    /// <summary>
    /// Тип об'єкта (наприклад: Apartment, House, Commercial).
    /// </summary>
    [Required(ErrorMessage = "Тип нерухомості є обов'язковим.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Тип нерухомості має містити від 2 до 50 символів.")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Кількість кімнат (від 1 до 20).
    /// </summary>
    [Required(ErrorMessage = "Кількість кімнат є обов'язковою.")]
    [Range(1, 20, ErrorMessage = "Кількість кімнат має бути від 1 до 20.")]
    public int RoomsCount { get; set; }

    /// <summary>
    /// Чи доступний об'єкт для нових угод.
    /// </summary>
    public bool IsAvailable { get; set; } = true;
}
