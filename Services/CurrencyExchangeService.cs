using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace WebActionResults.Services;

public sealed record ExchangeRateQuote(
    string BaseCurrency,
    string TargetCurrency,
    decimal Rate,
    DateTimeOffset UpdatedAtUtc,
    string Source,
    bool IsFallback);

public interface ICurrencyExchangeService
{
    Task<ExchangeRateQuote> GetRateAsync(string targetCurrency, CancellationToken cancellationToken = default);
}

public sealed class CurrencyExchangeService : ICurrencyExchangeService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ExchangeRateOptions _options;
    private readonly ILogger<CurrencyExchangeService> _logger;

    public CurrencyExchangeService(
        HttpClient httpClient,
        IMemoryCache cache,
        IOptions<ExchangeRateOptions> options,
        ILogger<CurrencyExchangeService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ExchangeRateQuote> GetRateAsync(string targetCurrency, CancellationToken cancellationToken = default)
    {
        var baseCurrency = NormalizeCurrency(_options.BaseCurrency, "VND");
        var target = NormalizeCurrency(targetCurrency, _options.DefaultDisplayCurrency);

        if (target == baseCurrency)
        {
            return new ExchangeRateQuote(baseCurrency, target, 1m, DateTimeOffset.UtcNow, "local", false);
        }

        var cacheKey = $"exchange-rate:{baseCurrency}:{target}";
        if (_cache.TryGetValue(cacheKey, out ExchangeRateQuote? cached) && cached is not null)
        {
            return cached;
        }

        try
        {
            var url = string.Format(CultureInfo.InvariantCulture, _options.ApiUrlTemplate, baseCurrency);
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var root = document.RootElement;

            if (root.TryGetProperty("rates", out var rates)
                && rates.TryGetProperty(target, out var rateElement)
                && rateElement.TryGetDecimal(out var rate)
                && rate > 0)
            {
                var updatedAt = DateTimeOffset.UtcNow;
                if (root.TryGetProperty("time_last_update_unix", out var unixElement)
                    && unixElement.TryGetInt64(out var unix))
                {
                    updatedAt = DateTimeOffset.FromUnixTimeSeconds(unix);
                }

                var quote = new ExchangeRateQuote(baseCurrency, target, rate, updatedAt, "open.er-api.com", false);
                _cache.Set(cacheKey, quote, TimeSpan.FromHours(Math.Max(1, _options.CacheHours)));
                return quote;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not refresh exchange rate {BaseCurrency}->{TargetCurrency}", baseCurrency, target);
        }

        var fallbackRate = GetFallbackRate(target);
        var fallbackQuote = new ExchangeRateQuote(baseCurrency, target, fallbackRate, DateTimeOffset.UtcNow, "fallback", true);
        _cache.Set(cacheKey, fallbackQuote, TimeSpan.FromHours(1));
        return fallbackQuote;
    }

    private decimal GetFallbackRate(string targetCurrency)
    {
        if (_options.FallbackRates.TryGetValue(targetCurrency, out var rate) && rate > 0)
        {
            return rate;
        }

        return targetCurrency.Equals("USD", StringComparison.OrdinalIgnoreCase) ? 0.000039m : 1m;
    }

    private static string NormalizeCurrency(string? value, string fallback)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim().ToUpperInvariant();
        return normalized.Length == 3 && normalized.All(char.IsLetter) ? normalized : fallback;
    }
}
