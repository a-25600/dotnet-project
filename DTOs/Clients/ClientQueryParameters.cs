namespace RealEstateApi.DTOs.Clients;

/// <summary>
/// Параметри пошуку та сортування клієнтів.
/// </summary>
public class ClientQueryParameters
{
    /// <summary>
    /// Пошуковий рядок (пошук за ПІБ, email або номером телефону).
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Поле для сортування: name, email, phone, id (за замовчуванням: name).
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Напрямок сортування: asc або desc (за замовчуванням: asc).
    /// </summary>
    public string SortOrder { get; set; } = "asc";
}
