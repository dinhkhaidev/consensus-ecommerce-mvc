namespace WebActionResults.Services.Ai;

public class AiOptions
{
    public string GeminiApiKey { get; set; } = string.Empty;
    public string GeminiModel { get; set; } = "gemini-3.1-preview";
    public int MaxCatalogItems { get; set; } = 120;
    public int CandidateCacheMinutes { get; set; } = 10;
    public bool EnableImage { get; set; } = true;
    public bool EnableAdminTools { get; set; } = true;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(GeminiApiKey);

    public static AiOptions FromEnvironment(IConfiguration configuration)
    {
        return new AiOptions
        {
            GeminiApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                ?? configuration["Gemini:ApiKey"]
                ?? string.Empty,
            GeminiModel = Environment.GetEnvironmentVariable("GEMINI_MODEL")
                ?? configuration["Gemini:Model"]
                ?? "gemini-3.1-preview",
            MaxCatalogItems = ReadInt("AI_MAX_CATALOG_ITEMS", configuration["Ai:MaxCatalogItems"], 120, 20, 300),
            CandidateCacheMinutes = ReadInt("AI_CANDIDATE_CACHE_MINUTES", configuration["Ai:CandidateCacheMinutes"], 10, 1, 120),
            EnableImage = ReadBool("AI_ENABLE_IMAGE", configuration["Ai:EnableImage"], true),
            EnableAdminTools = ReadBool("AI_ENABLE_ADMIN_TOOLS", configuration["Ai:EnableAdminTools"], true)
        };
    }

    private static int ReadInt(string envKey, string? configValue, int defaultValue, int min, int max)
    {
        var raw = Environment.GetEnvironmentVariable(envKey) ?? configValue;
        return int.TryParse(raw, out var value) ? Math.Clamp(value, min, max) : defaultValue;
    }

    private static bool ReadBool(string envKey, string? configValue, bool defaultValue)
    {
        var raw = Environment.GetEnvironmentVariable(envKey) ?? configValue;
        return bool.TryParse(raw, out var value) ? value : defaultValue;
    }
}
