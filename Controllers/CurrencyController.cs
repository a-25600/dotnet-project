using Microsoft.AspNetCore.Mvc;
using RealEstateApi.DTOs.Currency;
using RealEstateApi.Services.Common;
using RealEstateApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
namespace RealEstateApi.Controllers;

/// <summary>
/// Сервіс валютних курсів та конвертації (інтеграція з НБУ через HttpClientFactory).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class CurrencyController : ControllerBase
{
    private readonly ICurrencyService _currencyService;

    public CurrencyController(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

    /// <summary>
    /// Отримати офіційні актуальні курси валют від НБУ.
    /// </summary>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="200">Список курсів основних валют.</response>
    [HttpGet("rates")]
    [ProducesResponseType(typeof(List<CurrencyRateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRates(CancellationToken cancellationToken)
    {
        var result = await _currencyService.GetExchangeRatesAsync(cancellationToken);
        return Ok(result.Data);
    }

    /// <summary>
    /// Отримати курс конкретної валюти (наприклад: USD, EUR, PLN).
    /// </summary>
    /// <param name="code">Тризначний код валюти (ISO 4217).</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="200">Курс запитаної валюти.</response>
    /// <response code="404">Валюту з вказаним кодом не знайдено.</response>
    [HttpGet("rates/{code}")]
    [ProducesResponseType(typeof(CurrencyRateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRateByCode(string code, CancellationToken cancellationToken)
    {
        var result = await _currencyService.GetRateByCodeAsync(code, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Конвертувати суму між валютами за актуальним курсом (наприклад, вартість нерухомості з USD в UAH або EUR).
    /// </summary>
    /// <param name="amount">Сума для конвертації.</param>
    /// <param name="fromCurrency">Вихідна валюта (наприклад: USD).</param>
    /// <param name="toCurrency">Цільова валюта (наприклад: UAH).</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <response code="200">Результат конвертації.</response>
    /// <response code="400">Некоректні вхідні дані.</response>
    /// <response code="404">Вказану валюту не знайдено.</response>
    [HttpGet("convert")]
    [ProducesResponseType(typeof(CurrencyConversionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConvertCurrency(
        [FromQuery] decimal amount,
        [FromQuery] string fromCurrency = "USD",
        [FromQuery] string toCurrency = "UAH",
        CancellationToken cancellationToken = default)
    {
        var result = await _currencyService.ConvertAsync(amount, fromCurrency, toCurrency, cancellationToken);

        return result.Status switch
        {
            ServiceStatus.NotFound => NotFound(new { message = result.ErrorMessage }),
            ServiceStatus.BadRequest => BadRequest(new { message = result.ErrorMessage }),
            _ => Ok(result.Data)
        };
    }
}
