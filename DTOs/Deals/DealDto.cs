namespace RealEstateApi.DTOs.Deals;

/// <summary>
/// Представлення угоди для списків та відповідей API.
/// </summary>
public class DealDto
{
    /// <summary>
    /// Унікальний ідентифікатор угоди.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Ідентифікатор об'єкта нерухомості.
    /// </summary>
    public int PropertyId { get; set; }

    /// <summary>
    /// Адреса об'єкта нерухомості.
    /// </summary>
    public string PropertyAddress { get; set; } = string.Empty;

    /// <summary>
    /// Ідентифікатор клієнта.
    /// </summary>
    public int ClientId { get; set; }

    /// <summary>
    /// ПІБ клієнта.
    /// </summary>
    public string ClientName { get; set; } = string.Empty;

    /// <summary>
    /// Ідентифікатор ріелтора.
    /// </summary>
    public int RealtorId { get; set; }

    /// <summary>
    /// ПІБ ріелтора.
    /// </summary>
    public string RealtorName { get; set; } = string.Empty;

    /// <summary>
    /// Сума угоди.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Дата та час укладання угоди (UTC).
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Поточний статус угоди (Pending, Completed, Cancelled).
    /// </summary>
    public string Status { get; set; } = string.Empty;
}
