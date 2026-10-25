namespace RealEstateApi.DTOs.Deals;

/// <summary>
/// Короткий опис угоди для вкладених відповідей (клієнти, ріелтори, нерухомість).
/// </summary>
public class DealSummaryDto
{
    /// <summary>
    /// Ідентифікатор угоди.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Адреса об'єкта нерухомості.
    /// </summary>
    public string PropertyAddress { get; set; } = string.Empty;

    /// <summary>
    /// ПІБ клієнта.
    /// </summary>
    public string ClientName { get; set; } = string.Empty;

    /// <summary>
    /// ПІБ ріелтора.
    /// </summary>
    public string AgentName { get; set; } = string.Empty;

    /// <summary>
    /// Сума угоди.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Дата та час укладання угоди (UTC).
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Статус угоди (Pending, Completed, Cancelled).
    /// </summary>
    public string Status { get; set; } = string.Empty;
}