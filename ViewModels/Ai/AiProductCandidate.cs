namespace WebActionResults.ViewModels.Ai;

public class AiProductCandidate
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public IReadOnlyList<string> Colors { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Sizes { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();
    public double ScoreHint { get; set; }
}
