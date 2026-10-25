namespace RealEstateApi.DTOs.Properties;

/// <summary>
/// Представлення об'єкта нерухомості для списків та відповідей.
/// </summary>
public class PropertyDto
{
    /// <summary>
    /// Унікальний ідентифікатор об'єкта.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Адреса об'єкта нерухомості.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Вартість об'єкта.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Тип нерухомості (наприклад: Apartment, House, Commercial).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Кількість кімнат.
    /// </summary>
    public int RoomsCount { get; set; }

    /// <summary>
    /// Чи доступний об'єкт для оренди або продажу.
    /// </summary>
    public bool IsAvailable { get; set; } = true;
}
