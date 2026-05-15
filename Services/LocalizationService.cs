using System.Text.Json;

namespace WebActionResults.Services;

public interface ILocalizationService
{
    string CurrentLanguage { get; }
    string Get(string key);
    string Get(string key, params object[] args);
    void SetLanguage(string languageCode);
}

public class LocalizationService : ILocalizationService
{
    private static readonly string[] SupportedLanguages = ["en", "vi"];
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Dictionary<string, Dictionary<string, string>> _translations;

    public LocalizationService(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment)
    {
        _httpContextAccessor = httpContextAccessor;
        _translations = LoadTranslations(environment.ContentRootPath);
    }

    public string CurrentLanguage => DetectLanguage();

    private static Dictionary<string, Dictionary<string, string>> LoadTranslations(string contentRootPath)
    {
        var translations = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        var localizationPath = Path.Combine(contentRootPath, "Resources", "Localization");

        foreach (var language in SupportedLanguages)
        {
            var filePath = Path.Combine(localizationPath, $"{language}.json");
            translations[language] = File.Exists(filePath)
                ? LoadTranslationFile(filePath)
                : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        return translations;
    }

    private static Dictionary<string, string> LoadTranslationFile(string filePath)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        using var stream = File.OpenRead(filePath);
        using var document = JsonDocument.Parse(stream);

        if (document.RootElement.ValueKind == JsonValueKind.Object)
            FlattenJsonObject(document.RootElement, values);

        return values;
    }

    private static void FlattenJsonObject(JsonElement element, Dictionary<string, string> values, string? prefix = null)
    {
        foreach (var property in element.EnumerateObject())
        {
            var key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";

            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                FlattenJsonObject(property.Value, values, key);
                continue;
            }

            if (property.Value.ValueKind == JsonValueKind.String)
            {
                var value = property.Value.GetString() ?? string.Empty;
                values[key] = value;
                values[key.Replace(".", ":", StringComparison.Ordinal)] = value;
            }
        }
    }

    private string DetectLanguage()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return "en";

        var sessionLang = context.Session.GetString("Language");
        if (!string.IsNullOrEmpty(sessionLang) && _translations.ContainsKey(sessionLang))
            return sessionLang;

        var cookieLang = context.Request.Cookies["Language"];
        if (!string.IsNullOrEmpty(cookieLang) && _translations.ContainsKey(cookieLang))
            return cookieLang;

        var acceptLang = context.Request.Headers["Accept-Language"].FirstOrDefault();
        if (!string.IsNullOrEmpty(acceptLang))
        {
            if (acceptLang.Contains("vi", StringComparison.OrdinalIgnoreCase))
                return "vi";
            if (acceptLang.Contains("en", StringComparison.OrdinalIgnoreCase))
                return "en";
        }

        return "en";
    }

    public string Get(string key)
    {
        var lang = DetectLanguage();
        if (_translations.TryGetValue(lang, out var langDict) && langDict.TryGetValue(key, out var value))
            return value;

        if (_translations.TryGetValue("en", out var enDict) && enDict.TryGetValue(key, out value))
            return value;

        return key;
    }

    public string Get(string key, params object[] args)
    {
        var template = Get(key);
        return string.Format(template, args);
    }

    public void SetLanguage(string languageCode)
    {
        if (!_translations.ContainsKey(languageCode))
            return;

        var context = _httpContextAccessor.HttpContext;
        if (context == null)
            return;

        context.Session.SetString("Language", languageCode);
        context.Response.Cookies.Append("Language", languageCode, new CookieOptions
        {
            Expires = DateTimeOffset.Now.AddYears(1),
            SameSite = SameSiteMode.Lax
        });
    }
}
