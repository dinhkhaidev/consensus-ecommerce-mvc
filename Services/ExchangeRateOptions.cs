namespace WebActionResults.Services;

public sealed class ExchangeRateOptions
{
    public string BaseCurrency { get; set; } = "VND";
    public string DefaultDisplayCurrency { get; set; } = "USD";
    public string ApiUrlTemplate { get; set; } = "https://open.er-api.com/v6/latest/{0}";
    public int CacheHours { get; set; } = 12;
    public Dictionary<string, decimal> FallbackRates { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ["USD"] = 0.000039m
    };
}
