namespace RealEstateApi.DTOs.Deals;

/// <summary>
/// Параметри фільтрації та сортування списку угод.
/// </summary>
public class DealQueryParameters
{
    /// <summary>
    /// Фільтр за статусом угоди (Pending, Completed, Cancelled).
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Фільтр за конкретним об'єктом нерухомості.
    /// </summary>
    public int? PropertyId { get; set; }

    /// <summary>
    /// Фільтр за конкретним клієнтом.
    /// </summary>
    public int? ClientId { get; set; }

    /// <summary>
    /// Фільтр за конкретним ріелтором.
    /// </summary>
    public int? RealtorId { get; set; }

    /// <summary>
    /// Мінімальна сума угоди.
    /// </summary>
    public decimal? MinPrice { get; set; }

    /// <summary>
    /// Максимальна сума угоди.
    /// </summary>
    public decimal? MaxPrice { get; set; }

    /// <summary>
    /// Поле для сортування: date, price, status, id (за замовчуванням: date).
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Напрямок сортування: asc або desc (за замовчуванням: desc).
    /// </summary>
    public string? SortOrder { get; set; }
}
