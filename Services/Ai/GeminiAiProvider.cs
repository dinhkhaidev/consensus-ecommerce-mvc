using System.Net.Http.Json;
using System.Text.Json;

namespace WebActionResults.Services.Ai;

public class GeminiAiProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly AiOptions _options;
    private readonly ILogger<GeminiAiProvider> _logger;

    public GeminiAiProvider(HttpClient httpClient, AiOptions options, ILogger<GeminiAiProvider> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public bool IsConfigured => _options.IsConfigured;
    public string ModelName => _options.GeminiModel;

    public async Task<string> GenerateJsonAsync(string prompt, CancellationToken cancellationToken = default)
        => await GenerateJsonCoreAsync(new object[] { new { text = prompt } }, cancellationToken);

    public async Task<string> GenerateJsonWithImageAsync(string prompt, byte[] imageBytes, string mimeType, CancellationToken cancellationToken = default)
        => await GenerateJsonCoreAsync(
            new object[]
            {
                new { text = prompt },
                new
                {
                    inlineData = new
                    {
                        mimeType,
                        data = Convert.ToBase64String(imageBytes)
                    }
                }
            },
            cancellationToken);

    private async Task<string> GenerateJsonCoreAsync(object[] requestParts, CancellationToken cancellationToken)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("GEMINI_API_KEY is not configured.");

        var request = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = requestParts
                }
            },
            generationConfig = new
            {
                temperature = 0.25,
                topP = 0.85,
                responseMimeType = "application/json"
            }
        };

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(_options.GeminiModel)}:generateContent?key={Uri.EscapeDataString(_options.GeminiApiKey)}";
        using var response = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Gemini request failed with status {StatusCode}: {Body}", response.StatusCode, body);
            throw new InvalidOperationException("Gemini request failed.");
        }

        using var document = JsonDocument.Parse(body);
        var responseParts = document.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts");

        foreach (var part in responseParts.EnumerateArray())
        {
            if (part.TryGetProperty("text", out var textElement))
                return textElement.GetString() ?? "{}";
        }

        return "{}";
    }
}
