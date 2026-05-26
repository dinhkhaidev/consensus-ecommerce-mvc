namespace WebActionResults.Services.Ai;

public interface IAiProvider
{
    bool IsConfigured { get; }
    string ModelName { get; }
    Task<string> GenerateJsonAsync(string prompt, CancellationToken cancellationToken = default);
    Task<string> GenerateJsonWithImageAsync(string prompt, byte[] imageBytes, string mimeType, CancellationToken cancellationToken = default);
}
