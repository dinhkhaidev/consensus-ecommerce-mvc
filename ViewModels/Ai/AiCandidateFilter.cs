namespace WebActionResults.ViewModels.Ai;

public class AiCandidateFilter
{
    public string? RoomType { get; set; }
    public string? Style { get; set; }
    public decimal? Budget { get; set; }
    public string? UserDescription { get; set; }
    public string? Priority { get; set; }
    public IReadOnlyList<string> PreferredColors { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> AvoidColors { get; set; } = Array.Empty<string>();
    public int MaxCandidates { get; set; } = 120;
}
