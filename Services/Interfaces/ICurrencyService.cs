using RealEstateApi.DTOs.Currency;
using RealEstateApi.Services.Common;

namespace RealEstateApi.Services.Interfaces;

/// <summary>
/// Зовнішній HTTP-сервіс курсів валют та конвертації (інтеграція з відкритим API Національного банку України).
/// </summary>
public interface ICurrencyService
{
    /// <summary>
    /// Отримати актуальні офіційні курси валют до гривні (USD, EUR, PLN, GBP тощо).
    /// </summary>
    Task<ServiceResult<IReadOnlyList<CurrencyRateDto>>> GetExchangeRatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Отримати курс конкретної валюти за її кодом (USD, EUR тощо).
    /// </summary>
    Task<ServiceResult<CurrencyRateDto>> GetRateByCodeAsync(string currencyCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Конвертувати суму з однієї валюти в іншу за актуальним курсом.
    /// </summary>
    Task<ServiceResult<CurrencyConversionResultDto>> ConvertAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken cancellationToken = default);
}
