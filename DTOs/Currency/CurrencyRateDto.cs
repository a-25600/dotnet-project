namespace RealEstateApi.DTOs.Currency;

/// <summary>
/// Курс валюти відносно базової валюти (UAH).
/// </summary>
public class CurrencyRateDto
{
    /// <summary>
    /// Літерний код валюти (наприклад: USD, EUR, PLN, GBP).
    /// </summary>
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// Назва валюти.
    /// </summary>
    public string CurrencyName { get; set; } = string.Empty;

    /// <summary>
    /// Офіційний курс до гривні (UAH).
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// Дата встановлення курсу.
    /// </summary>
    public string ExchangeDate { get; set; } = string.Empty;
}
