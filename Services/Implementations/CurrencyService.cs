using System.Net.Http.Json;
using System.Text.Json.Serialization;
using RealEstateApi.DTOs.Currency;
using RealEstateApi.Services.Common;
using RealEstateApi.Services.Interfaces;

namespace RealEstateApi.Services.Implementations;

/// <summary>
/// Реалізація зовнішнього HTTP-сервісу отримання курсів валют від API Національного банку України з використанням HttpClientFactory.
/// </summary>
public class CurrencyService : ICurrencyService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CurrencyService> _logger;

    private static readonly Dictionary<string, (string Name, decimal Rate)> FallbackRates = new(StringComparer.OrdinalIgnoreCase)
    {
        ["USD"] = ("Долар США", 41.50m),
        ["EUR"] = ("Євро", 44.80m),
        ["PLN"] = ("Польський злотий", 10.45m),
        ["GBP"] = ("Фунт стерлінгів", 53.60m),
        ["CHF"] = ("Швейцарський франк", 47.90m)
    };

    public CurrencyService(HttpClient httpClient, ILogger<CurrencyService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ServiceResult<IReadOnlyList<CurrencyRateDto>>> GetExchangeRatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<NbuRateItem>>(
                "NBUStatService/v1/statdirectory/exchange?json",
                cancellationToken);

            if (response == null || response.Count == 0)
            {
                _logger.LogWarning("NBU API returned empty list of rates, using fallback values.");
                return ServiceResult<IReadOnlyList<CurrencyRateDto>>.Ok(GetFallbackRates());
            }

            var supportedCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "USD", "EUR", "PLN", "GBP", "CHF"
            };

            var rates = response
                .Where(r => supportedCodes.Contains(r.Cc))
                .Select(r => new CurrencyRateDto
                {
                    CurrencyCode = r.Cc.ToUpperInvariant(),
                    CurrencyName = r.Txt,
                    Rate = r.Rate,
                    ExchangeDate = r.ExchangeDate
                })
                .OrderBy(r => r.CurrencyCode)
                .ToList();

            return ServiceResult<IReadOnlyList<CurrencyRateDto>>.Ok(rates);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch exchange rates from NBU API. Using fallback rates.");
            return ServiceResult<IReadOnlyList<CurrencyRateDto>>.Ok(GetFallbackRates());
        }
    }

    public async Task<ServiceResult<CurrencyRateDto>> GetRateByCodeAsync(string currencyCode, CancellationToken cancellationToken = default)
    {
        var code = currencyCode.Trim().ToUpperInvariant();

        if (code == "UAH")
        {
            return ServiceResult<CurrencyRateDto>.Ok(new CurrencyRateDto
            {
                CurrencyCode = "UAH",
                CurrencyName = "Гривня",
                Rate = 1.0m,
                ExchangeDate = DateTime.UtcNow.ToString("dd.MM.yyyy")
            });
        }

        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<NbuRateItem>>(
                $"NBUStatService/v1/statdirectory/exchange?valcode={code}&json",
                cancellationToken);

            var item = response?.FirstOrDefault();
            if (item != null)
            {
                return ServiceResult<CurrencyRateDto>.Ok(new CurrencyRateDto
                {
                    CurrencyCode = item.Cc.ToUpperInvariant(),
                    CurrencyName = item.Txt,
                    Rate = item.Rate,
                    ExchangeDate = item.ExchangeDate
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch rate for {Code} from NBU API. Checking fallback rates.", code);
        }

        if (FallbackRates.TryGetValue(code, out var fallback))
        {
            return ServiceResult<CurrencyRateDto>.Ok(new CurrencyRateDto
            {
                CurrencyCode = code,
                CurrencyName = fallback.Name,
                Rate = fallback.Rate,
                ExchangeDate = DateTime.UtcNow.ToString("dd.MM.yyyy")
            });
        }

        return ServiceResult<CurrencyRateDto>.NotFound($"Exchange rate for currency '{currencyCode}' not found.");
    }

    public async Task<ServiceResult<CurrencyConversionResultDto>> ConvertAsync(
        decimal amount,
        string fromCurrency,
        string toCurrency,
        CancellationToken cancellationToken = default)
    {
        if (amount < 0)
            return ServiceResult<CurrencyConversionResultDto>.BadRequest("Сума для конвертації не може бути від'ємною.");

        var from = fromCurrency.Trim().ToUpperInvariant();
        var to = toCurrency.Trim().ToUpperInvariant();

        if (from == to)
        {
            return ServiceResult<CurrencyConversionResultDto>.Ok(new CurrencyConversionResultDto
            {
                SourceAmount = amount,
                FromCurrency = from,
                ToCurrency = to,
                ConvertedAmount = amount,
                ExchangeRate = 1.0m,
                RateDate = DateTime.UtcNow.ToString("dd.MM.yyyy")
            });
        }

        decimal fromRateToUah = 1.0m;
        string rateDate = DateTime.UtcNow.ToString("dd.MM.yyyy");

        if (from != "UAH")
        {
            var fromResult = await GetRateByCodeAsync(from, cancellationToken);
            if (!fromResult.IsSuccess || fromResult.Data == null)
                return ServiceResult<CurrencyConversionResultDto>.NotFound($"Currency '{from}' is not supported.");

            fromRateToUah = fromResult.Data.Rate;
            rateDate = fromResult.Data.ExchangeDate;
        }

        decimal toRateToUah = 1.0m;
        if (to != "UAH")
        {
            var toResult = await GetRateByCodeAsync(to, cancellationToken);
            if (!toResult.IsSuccess || toResult.Data == null)
                return ServiceResult<CurrencyConversionResultDto>.NotFound($"Currency '{to}' is not supported.");

            toRateToUah = toResult.Data.Rate;
            rateDate = toResult.Data.ExchangeDate;
        }

        var crossRate = fromRateToUah / toRateToUah;
        var convertedAmount = Math.Round(amount * crossRate, 2);

        return ServiceResult<CurrencyConversionResultDto>.Ok(new CurrencyConversionResultDto
        {
            SourceAmount = amount,
            FromCurrency = from,
            ToCurrency = to,
            ConvertedAmount = convertedAmount,
            ExchangeRate = Math.Round(crossRate, 4),
            RateDate = rateDate
        });
    }

    private static List<CurrencyRateDto> GetFallbackRates()
    {
        var date = DateTime.UtcNow.ToString("dd.MM.yyyy");
        return FallbackRates.Select(kv => new CurrencyRateDto
        {
            CurrencyCode = kv.Key,
            CurrencyName = kv.Value.Name,
            Rate = kv.Value.Rate,
            ExchangeDate = date
        }).OrderBy(r => r.CurrencyCode).ToList();
    }

    private sealed class NbuRateItem
    {
        [JsonPropertyName("r030")]
        public int R030 { get; set; }

        [JsonPropertyName("txt")]
        public string Txt { get; set; } = string.Empty;

        [JsonPropertyName("rate")]
        public decimal Rate { get; set; }

        [JsonPropertyName("cc")]
        public string Cc { get; set; } = string.Empty;

        [JsonPropertyName("exchangedate")]
        public string ExchangeDate { get; set; } = string.Empty;
    }
}
