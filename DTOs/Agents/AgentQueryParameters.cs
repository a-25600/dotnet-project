namespace RealEstateApi.DTOs.Agents;

/// <summary>
/// Параметри фільтрації, пошуку та сортування ріелторів.
/// </summary>
public class AgentQueryParameters
{
    /// <summary>
    /// Фільтр за активністю співробітника (true - тільки активні, false - неактивні).
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Пошуковий рядок (пошук за ПІБ, email, телефоном або номером ліцензії).
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Поле для сортування: name, email, phone, license, isactive, id (за замовчуванням: name).
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Напрямок сортування: asc або desc (за замовчуванням: asc).
    /// </summary>
    public string SortOrder { get; set; } = "asc";
}