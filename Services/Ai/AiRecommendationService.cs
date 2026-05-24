using WebActionResults.ViewModels.Ai;

namespace WebActionResults.Services.Ai;

public interface IAiRecommendationService
{
    bool IsAiConfigured { get; }
    Task<(AiRecommendationResult Result, IReadOnlyList<AiProductCandidate> Candidates)> RecommendFromStyleSurveyAsync(
        StyleSurveyRequest request,
        CancellationToken cancellationToken = default);
}

public class AiRecommendationService : IAiRecommendationService
{
    private readonly IAiProductCandidateService _candidateService;
    private readonly IAiProvider _aiProvider;
    private readonly IAiPromptBuilder _promptBuilder;
    private readonly IAiJsonParser _jsonParser;
    private readonly AiOptions _options;
    private readonly ILogger<AiRecommendationService> _logger;

    public AiRecommendationService(
        IAiProductCandidateService candidateService,
        IAiProvider aiProvider,
        IAiPromptBuilder promptBuilder,
        IAiJsonParser jsonParser,
        AiOptions options,
        ILogger<AiRecommendationService> logger)
    {
        _candidateService = candidateService;
        _aiProvider = aiProvider;
        _promptBuilder = promptBuilder;
        _jsonParser = jsonParser;
        _options = options;
        _logger = logger;
    }

    public bool IsAiConfigured => _aiProvider.IsConfigured;

    public async Task<(AiRecommendationResult Result, IReadOnlyList<AiProductCandidate> Candidates)> RecommendFromStyleSurveyAsync(
        StyleSurveyRequest request,
        CancellationToken cancellationToken = default)
    {
        var filter = new AiCandidateFilter
        {
            RoomType = request.RoomType,
            Style = request.Style,
            Budget = request.Budget,
            UserDescription = request.UserDescription,
            Priority = request.Priority,
            PreferredColors = SplitList(request.PreferredColors),
            AvoidColors = SplitList(request.AvoidColors),
            MaxCandidates = _options.MaxCatalogItems
        };

        var candidates = await _candidateService.GetCandidatesAsync(filter, cancellationToken);
        if (!candidates.Any())
        {
            return (new AiRecommendationResult
            {
                ConceptName = "Chưa đủ dữ liệu",
                Summary = "Hiện danh sách sản phẩm chưa có món phù hợp để tạo bản phối. Hãy thêm sản phẩm còn hàng rồi thử lại.",
                UsedFallback = true
            }, candidates);
        }

        if (!_aiProvider.IsConfigured)
            return (BuildFallbackResult(request, candidates), candidates);

        try
        {
            var prompt = _promptBuilder.BuildStyleSurveyPrompt(request, candidates);
            var rawJson = await _aiProvider.GenerateJsonAsync(prompt, cancellationToken);
            var result = _jsonParser.ParseRecommendation(rawJson);
            return (result, candidates);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI recommendation failed. Falling back to deterministic candidate ranking.");
            return (BuildFallbackResult(request, candidates), candidates);
        }
    }

    private static AiRecommendationResult BuildFallbackResult(StyleSurveyRequest request, IReadOnlyList<AiProductCandidate> candidates)
    {
        var recommended = candidates
            .Take(8)
            .Select((candidate, index) => new AiRecommendedProduct
            {
                ProductId = candidate.ProductId,
                Rank = index + 1,
                Reason = "Phù hợp với ngân sách, nhóm sản phẩm và tình trạng còn hàng."
            })
            .ToList();

        return new AiRecommendationResult
        {
            ConceptName = "Phối nhanh từ Consensus",
            Summary = $"Đây là bản phối nhanh dựa trên các món đang còn hàng cho {Humanize(request.RoomType)}. Khi bật tư vấn AI, phần phong cách và lý do chọn từng món sẽ được cá nhân hóa kỹ hơn.",
            Palette = new[] { "gỗ ấm", "màu trung tính", "ánh sáng mềm" },
            RecommendedProducts = recommended,
            UsedFallback = true
        };
    }

    private static IReadOnlyList<string> SplitList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Array.Empty<string>();

        return value
            .Split(new[] { ',', ';', '/', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(item => item.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string Humanize(string? value)
        => string.IsNullOrWhiteSpace(value) ? "không gian của bạn" : value.Replace("_", " ");
}
