namespace RealEstateApi.DTOs.Properties;

/// <summary>
/// Параметри фільтрації, пошуку та сортування об'єктів нерухомості.
/// </summary>
public class PropertyQueryParameters
{
    /// <summary>
    /// Мінімальна вартість об'єкта.
    /// </summary>
    public decimal? MinPrice { get; set; }

    /// <summary>
    /// Максимальна вартість об'єкта.
    /// </summary>
    public decimal? MaxPrice { get; set; }

    /// <summary>
    /// Мінімальна кількість кімнат.
    /// </summary>
    public int? MinRooms { get; set; }

    /// <summary>
    /// Максимальна кількість кімнат.
    /// </summary>
    public int? MaxRooms { get; set; }

    /// <summary>
    /// Фільтр за типом нерухомості (наприклад: Apartment, House).
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Фільтр за доступністю (true - доступний, false - недоступний).
    /// </summary>
    public bool? IsAvailable { get; set; }

    /// <summary>
    /// Пошуковий рядок (пошук за адресою або типом).
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Поле для сортування: price, rooms, address, type, id (за замовчуванням: id).
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Напрямок сортування: asc або desc (за замовчуванням: asc).
    /// </summary>
    public string? SortOrder { get; set; }
}
