using System.ComponentModel.DataAnnotations;

namespace WebActionResults.ViewModels.Ai;

public class StyleSurveyRequest
{
    [Display(Name = "Khu vực")]
    public string RoomType { get; set; } = string.Empty;

    [Display(Name = "Phong cách")]
    public string Style { get; set; } = string.Empty;

    [Range(0, 999999999)]
    public decimal? Budget { get; set; }

    public string Area { get; set; } = string.Empty;

    [MaxLength(1200)]
    [Display(Name = "Mô tả căn phòng")]
    public string UserDescription { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string PreferredColors { get; set; } = string.Empty;

    public string AvoidColors { get; set; } = string.Empty;
}

public class AiRecommendationResult
{
    public string ConceptName { get; set; } = "Consensus Mix";
    public string Summary { get; set; } = string.Empty;
    public IReadOnlyList<string> Palette { get; set; } = Array.Empty<string>();
    public IReadOnlyList<AiRecommendedProduct> RecommendedProducts { get; set; } = Array.Empty<AiRecommendedProduct>();
    public bool UsedFallback { get; set; }
}

public class AiRecommendedProduct
{
    public int ProductId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int Rank { get; set; }
}

public class AiRecommendedProductViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int Rank { get; set; }
    public bool HasRoom3DModel { get; set; }
}

public class AiStylistPageViewModel
{
    public StyleSurveyRequest Request { get; set; } = new();
    public AiRecommendationResult? Result { get; set; }
    public IReadOnlyList<AiRecommendedProductViewModel> Products { get; set; } = Array.Empty<AiRecommendedProductViewModel>();
    public IReadOnlyList<AiStylistChatMessage> ChatHistory { get; set; } = Array.Empty<AiStylistChatMessage>();
    public string? ErrorMessage { get; set; }
    public bool IsAiConfigured { get; set; }
    public int CandidateCount { get; set; }
}

public class AiStylistChatMessage
{
    public string Role { get; set; } = "assistant";
    public string Mode { get; set; } = "Mô tả căn phòng";
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ProductAdvisorRequest
{
    public int ProductId { get; set; }
    public string NeedDescription { get; set; } = string.Empty;
    public decimal? Budget { get; set; }
    public string RoomType { get; set; } = string.Empty;
}

public class ProductAdvisorResult
{
    public string Verdict { get; set; } = "consider";
    public string VerdictLabel { get; set; } = "Nên cân nhắc";
    public int Confidence { get; set; } = 70;
    public string Summary { get; set; } = string.Empty;
    public IReadOnlyList<string> FitNotes { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> WatchOuts { get; set; } = Array.Empty<string>();
    public IReadOnlyList<AiRecommendedProductViewModel> BetterMatches { get; set; } = Array.Empty<AiRecommendedProductViewModel>();
    public bool UsedFallback { get; set; }
}
