using System.ComponentModel.DataAnnotations;

namespace RealEstateApi.DTOs.Deals;

/// <summary>
/// Дані для оновлення параметрів угоди.
/// </summary>
public class UpdateDealDto
{
    /// <summary>
    /// Оновлена сума угоди.
    /// </summary>
    [Required(ErrorMessage = "Ціна є обов'язковою.")]
    [Range(1, (double)decimal.MaxValue, ErrorMessage = "Сума угоди має бути більшою за 0.")]
    public decimal Price { get; set; }

    /// <summary>
    /// Статус угоди (Pending, Completed, Cancelled).
    /// </summary>
    [Required(ErrorMessage = "Статус є обов'язковим.")]
    [RegularExpression("^(?i)(Pending|Completed|Cancelled)$", ErrorMessage = "Статус може мати лише значення: Pending, Completed, Cancelled.")]
    public string Status { get; set; } = string.Empty;
}