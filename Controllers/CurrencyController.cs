using Microsoft.AspNetCore.Mvc;
using WebActionResults.Services;

namespace WebActionResults.Controllers;

public class CurrencyController : Controller
{
    private readonly ICurrencyExchangeService _currencyExchangeService;

    public CurrencyController(ICurrencyExchangeService currencyExchangeService)
    {
        _currencyExchangeService = currencyExchangeService;
    }

    [HttpGet]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    public async Task<IActionResult> Rate(string target = "USD", CancellationToken cancellationToken = default)
    {
        var normalizedTarget = NormalizeCurrency(target);
        var quote = await _currencyExchangeService.GetRateAsync(normalizedTarget, cancellationToken);

        return Json(new
        {
            baseCurrency = quote.BaseCurrency,
            targetCurrency = quote.TargetCurrency,
            rate = quote.Rate,
            updatedAtUtc = quote.UpdatedAtUtc,
            source = quote.Source,
            isFallback = quote.IsFallback
        });
    }

    private static string NormalizeCurrency(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "USD" : value.Trim().ToUpperInvariant();
        return normalized.Length == 3 && normalized.All(char.IsLetter) ? normalized : "USD";
    }
}
