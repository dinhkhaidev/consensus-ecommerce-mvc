using System.Text.Json;
using System.Text.RegularExpressions;
using WebActionResults.ViewModels.Ai;

namespace WebActionResults.Services.Ai;

public interface IAiJsonParser
{
    AiRecommendationResult ParseRecommendation(string rawJson);
}

public class AiJsonParser : IAiJsonParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AiRecommendationResult ParseRecommendation(string rawJson)
    {
        var json = ExtractJson(rawJson);
        var result = JsonSerializer.Deserialize<AiRecommendationResult>(json, JsonOptions);
        return result ?? new AiRecommendationResult();
    }

    private static string ExtractJson(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
            return trimmed;

        var match = Regex.Match(trimmed, "\\{[\\s\\S]*\\}");
        return match.Success ? match.Value : "{}";
    }
}
