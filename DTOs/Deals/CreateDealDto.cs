using System.ComponentModel.DataAnnotations;

namespace RealEstateApi.DTOs.Deals;

/// <summary>
/// Дані для укладання нової угоди.
/// </summary>
public class CreateDealDto
{
    /// <summary>
    /// Ідентифікатор об'єкта нерухомості.
    /// </summary>
    [Required(ErrorMessage = "PropertyId є обов'язковим.")]
    [Range(1, int.MaxValue, ErrorMessage = "Некоректний PropertyId.")]
    public int PropertyId { get; set; }

    /// <summary>
    /// Ідентифікатор клієнта, який укладає угоду.
    /// </summary>
    [Required(ErrorMessage = "ClientId є обов'язковим.")]
    [Range(1, int.MaxValue, ErrorMessage = "Некоректний ClientId.")]
    public int ClientId { get; set; }

    /// <summary>
    /// Ідентифікатор відповідального ріелтора.
    /// </summary>
    [Required(ErrorMessage = "RealtorId є обов'язковим.")]
    [Range(1, int.MaxValue, ErrorMessage = "Некоректний RealtorId.")]
    public int RealtorId { get; set; }

    /// <summary>
    /// Сума угоди (має бути більша за 0).
    /// </summary>
    [Required(ErrorMessage = "Ціна є обов'язковою.")]
    [Range(1, (double)decimal.MaxValue, ErrorMessage = "Сума угоди має бути більшою за 0.")]
    public decimal Price { get; set; }
}