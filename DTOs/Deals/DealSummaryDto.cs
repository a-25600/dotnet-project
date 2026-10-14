namespace RealEstateApi.DTOs.Deals;

/// <summary>
/// Короткий опис угоди для вкладених відповідей
/// </summary>
public class DealSummaryDto
{
    public int Id { get; set; }
    public string PropertyAddress { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
}