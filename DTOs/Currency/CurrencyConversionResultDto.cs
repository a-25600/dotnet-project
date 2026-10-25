namespace RealEstateApi.DTOs.Currency;

/// <summary>
/// Результат конвертації суми між двома валютами.
/// </summary>
public class CurrencyConversionResultDto
{
    /// <summary>
    /// Початкова сума.
    /// </summary>
    public decimal SourceAmount { get; set; }

    /// <summary>
    /// Вихідна валюта.
    /// </summary>
    public string FromCurrency { get; set; } = string.Empty;

    /// <summary>
    /// Цільова валюта.
    /// </summary>
    public string ToCurrency { get; set; } = string.Empty;

    /// <summary>
    /// Розрахована сума у цільовій валюті.
    /// </summary>
    public decimal ConvertedAmount { get; set; }

    /// <summary>
    /// Застосований курс обміну.
    /// </summary>
    public decimal ExchangeRate { get; set; }

    /// <summary>
    /// Дата встановлення курсу.
    /// </summary>
    public string RateDate { get; set; } = string.Empty;
}
