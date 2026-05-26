using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WebActionResults.Data.Entities;
using WebActionResults.Data.Services;
using WebActionResults.Models;
using WebActionResults.Services.Ai;
using WebActionResults.Utilities;
using WebActionResults.Services;
using WebActionResults.ViewModels.Ai;

namespace WebActionResults.Controllers;

public class AiController : Controller
{
    private const string AiRoom3DSelectionSessionKey = "AI_ROOM3D_PRODUCT_IDS";
    private const string AiStylistLastResultSessionKey = "AI_STYLIST_LAST_RESULT";
    private const string AiStylistChatHistorySessionKey = "AI_STYLIST_CHAT_HISTORY";
    private const string AiImageCatalogDetailsCachePrefix = "ai:image-catalog-details:";
    private static readonly TimeSpan ProductAdvisorAiTimeout = TimeSpan.FromSeconds(10);
    private readonly IAiRecommendationService _recommendationService;
    private readonly IAiProductCandidateService _candidateService;
    private readonly IAiRecommendationValidator _validator;
    private readonly ICartService _cartService;
    private readonly IUserService _userService;
    private readonly ShopDbContext _context;
    private readonly IAiProvider _aiProvider;
    private readonly IAiPromptBuilder _aiPromptBuilder;
    private readonly IAiJsonParser _aiJsonParser;
    private readonly ILocalizationService _localizer;
    private readonly IMemoryCache _cache;

    public AiController(
        IAiRecommendationService recommendationService,
        IAiProductCandidateService candidateService,
        IAiRecommendationValidator validator,
        ICartService cartService,
        IUserService userService,
        ShopDbContext context,
        IAiProvider aiProvider,
        IAiPromptBuilder aiPromptBuilder,
        IAiJsonParser aiJsonParser,
        ILocalizationService localizer,
        IMemoryCache cache)
    {
        _recommendationService = recommendationService;
        _candidateService = candidateService;
        _validator = validator;
        _cartService = cartService;
        _userService = userService;
        _context = context;
        _aiProvider = aiProvider;
        _aiPromptBuilder = aiPromptBuilder;
        _aiJsonParser = aiJsonParser;
        _localizer = localizer;
        _cache = cache;
    }

    [HttpGet]
    public async Task<IActionResult> Stylist(int? productId, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Consensus Stylist";

        if (productId.HasValue)
        {
            var seededViewModel = await BuildProductSeedViewModelAsync(productId.Value, cancellationToken);
            if (seededViewModel != null)
            {
                return View(seededViewModel);
            }
        }

        var state = LoadLastStylistState();
        if (state?.Result == null)
        {
            return View(new AiStylistPageViewModel
            {
                IsAiConfigured = _recommendationService.IsAiConfigured,
                ChatHistory = LoadChatHistory()
            });
        }

        var products = await RebuildProductsFromSavedResultAsync(state.Result, cancellationToken);
        return View(new AiStylistPageViewModel
        {
            Request = state.Request ?? new StyleSurveyRequest(),
            Result = state.Result,
            Products = products,
            IsAiConfigured = _recommendationService.IsAiConfigured,
            CandidateCount = state.CandidateCount,
            ChatHistory = LoadChatHistory(),
            ErrorMessage = products.Any() ? null : _localizer.Get("AiStylist:ErrPrevComboUnavailable")
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Stylist(StyleSurveyRequest request, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Consensus Stylist";

        // Clear validation errors on optional fields that may fail binding
        foreach (var key in ModelState.Keys.ToList())
        {
            if (!string.Equals(key, nameof(request.UserDescription), StringComparison.OrdinalIgnoreCase))
                ModelState.Remove(key);
        }

        if (!string.IsNullOrEmpty(request.UserDescription) && request.UserDescription.Length > 1200)
        {
            return View(new AiStylistPageViewModel
            {
                Request = request,
                IsAiConfigured = _recommendationService.IsAiConfigured,
                ChatHistory = LoadChatHistory(),
                ErrorMessage = _localizer.Get("AiStylist:ErrDescriptionTooLong")
            });
        }

        var (result, candidates) = await _recommendationService.RecommendFromStyleSurveyAsync(request, cancellationToken);
        var products = await _validator.BuildValidatedProductsAsync(result, candidates, cancellationToken);
        SaveLastStylistState(request, result, candidates.Count);
        var chatHistory = AppendChatTurn(request, result, products.Count);

        return View(new AiStylistPageViewModel
        {
            Request = request,
            Result = result,
            Products = products,
            IsAiConfigured = _recommendationService.IsAiConfigured,
            CandidateCount = candidates.Count,
            ChatHistory = chatHistory,
            ErrorMessage = products.Any() ? null : _localizer.Get("AiStylist:ErrNoMatchingProducts")
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StylistApi([FromBody] StyleSurveyRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!string.IsNullOrEmpty(request.UserDescription) && request.UserDescription.Length > 1200)
                return Json(new { ok = false, error = _localizer.Get("AiStylist:ErrDescriptionTooLong") });

            var history = LoadChatHistory();
            var prompt = _aiPromptBuilder.BuildConversationPrompt(request.UserDescription, history);
            var rawResponse = await _aiProvider.GenerateJsonAsync(prompt, cancellationToken);

            string cleanJson = ExtractJson(rawResponse);
            using var doc = JsonDocument.Parse(cleanJson);
            var root = doc.RootElement;
            var action = root.GetProperty("action").GetString();
            var reply = root.GetProperty("reply").GetString() ?? "";

            if (string.Equals(action, "recommend", StringComparison.OrdinalIgnoreCase))
            {
                var pref = root.GetProperty("preferences");
                var surveyReq = new StyleSurveyRequest();
                if (pref.TryGetProperty("roomType", out var prop) && prop.ValueKind == JsonValueKind.String)
                    surveyReq.RoomType = prop.GetString() ?? "";
                if (pref.TryGetProperty("style", out prop) && prop.ValueKind == JsonValueKind.String)
                    surveyReq.Style = prop.GetString() ?? "";
                if (pref.TryGetProperty("budget", out prop))
                {
                    if (prop.ValueKind == JsonValueKind.Number)
                        surveyReq.Budget = prop.GetDecimal();
                    else if (prop.ValueKind == JsonValueKind.String && decimal.TryParse(prop.GetString(), out var d))
                        surveyReq.Budget = d;
                }
                if (pref.TryGetProperty("area", out prop) && prop.ValueKind == JsonValueKind.String)
                    surveyReq.Area = prop.GetString() ?? "";
                if (pref.TryGetProperty("priority", out prop) && prop.ValueKind == JsonValueKind.String)
                    surveyReq.Priority = prop.GetString() ?? "";
                if (pref.TryGetProperty("preferredColors", out prop) && prop.ValueKind == JsonValueKind.String)
                    surveyReq.PreferredColors = prop.GetString() ?? "";
                if (pref.TryGetProperty("avoidColors", out prop) && prop.ValueKind == JsonValueKind.String)
                    surveyReq.AvoidColors = prop.GetString() ?? "";

                if (string.IsNullOrWhiteSpace(surveyReq.UserDescription))
                    surveyReq.UserDescription = request.UserDescription;

                var (result, candidates) = await _recommendationService.RecommendFromStyleSurveyAsync(surveyReq, cancellationToken);
                var products = await _validator.BuildValidatedProductsAsync(result, candidates, cancellationToken);
                SaveLastStylistState(surveyReq, result, candidates.Count);
                AppendChatTurnForText(request.UserDescription, reply);

                return Json(new
                {
                    ok = true,
                    type = "recommend",
                    userMessage = request.UserDescription,
                    assistantMessage = reply,
                    result = new
                    {
                        conceptName = result.ConceptName,
                        summary = result.Summary,
                        palette = result.Palette,
                        usedFallback = result.UsedFallback
                    },
                    products = products.Select(p => new
                    {
                        productId = p.ProductId,
                        productName = p.ProductName,
                        categoryName = p.CategoryName,
                        unitPrice = p.UnitPrice,
                        stockQuantity = p.StockQuantity,
                        imageUrl = p.ImageUrl,
                        reason = p.Reason,
                        rank = p.Rank,
                        hasRoom3DModel = p.HasRoom3DModel
                    }),
                    candidateCount = candidates.Count,
                    error = (string?)null
                });
            }
            else
            {
                AppendChatTurnForText(request.UserDescription, reply);
                return Json(new
                {
                    ok = true,
                    type = "chat",
                    userMessage = request.UserDescription,
                    assistantMessage = reply,
                    error = (string?)null
                });
            }
        }
        catch (Exception)
        {
            // Simple fallback if anything fails (like JSON extraction or Gemini network errors)
            var fallbackReply = _localizer.Get("AiStylist:FallbackReply");
            AppendChatTurnForText(request.UserDescription, fallbackReply);
            return Json(new
            {
                ok = true,
                type = "chat",
                userMessage = request.UserDescription,
                assistantMessage = fallbackReply,
                error = (string?)null
            });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StylistImageApi(
        [FromForm] string? userDescription,
        [FromForm] string? roomType,
        [FromForm] string? style,
        [FromForm] decimal? budget,
        [FromForm] string? area,
        [FromForm] string? priority,
        [FromForm] string? preferredColors,
        [FromForm] string? avoidColors,
        [FromForm] IFormFile? image,
        CancellationToken cancellationToken)
    {
        if (!_aiProvider.IsConfigured)
            return Json(new { ok = false, error = "AI chưa sẵn sàng để soi ảnh lúc này." });

        if (image == null)
            return Json(new { ok = false, error = "Bạn gửi thêm ảnh món đồ để mình tìm đồ tương tự nhé." });

        var imageValidation = ValidateAdvisorImage(image);
        if (imageValidation != null)
            return Json(new { ok = false, error = imageValidation });

        var request = new ProductAdvisorRequest
        {
            NeedDescription = userDescription ?? string.Empty,
            Budget = budget,
            RoomType = roomType ?? string.Empty
        };

        var candidateFilter = BuildImageCandidateFilter(
            userDescription,
            roomType,
            style,
            budget,
            priority,
            preferredColors,
            avoidColors);
        var catalog = await LoadImageAdvisorCatalogAsync(null, candidateFilter, cancellationToken);

        await using var stream = image.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);

        try
        {
            var prompt = BuildProductImageAdvisorPrompt(null, request, catalog, new
            {
                style,
                area,
                priority,
                preferredColors,
                avoidColors
            });
            var raw = await _aiProvider.GenerateJsonWithImageAsync(prompt, memory.ToArray(), image.ContentType, cancellationToken);
            using var document = JsonDocument.Parse(ExtractJson(raw));
            var root = document.RootElement;

            var matchReasons = GetImageMatchReasons(root);
            var products = GetJsonIntArray(root, "matchingProductIds")
                .Select(id => catalog.FirstOrDefault(product => product.Id == id))
                .Where(product => product != null)
                .Cast<Product>()
                .Take(6)
                .Select((product, index) => ToAdvisorProductViewModel(
                    new AdvisorScoredProduct(product, 0, matchReasons.GetValueOrDefault(product.Id, "Gần với ảnh bạn gửi.")),
                    index + 1))
                .ToList();

            var reply = SanitizeAdvisorText(GetJsonString(root, "summary", "Mình đã soi ảnh và chọn ra vài món gần vibe nhất trong catalog."));
            var userMessage = string.IsNullOrWhiteSpace(userDescription)
                ? "Đã gửi ảnh để tìm sản phẩm tương tự."
                : userDescription;

            return Json(new
            {
                ok = true,
                type = "recommend",
                userMessage,
                assistantMessage = reply,
                result = new
                {
                    conceptName = GetJsonString(root, "headline", "Gợi ý từ ảnh bạn gửi"),
                    summary = reply,
                    palette = GetJsonStringArray(root, "visualNotes", Array.Empty<string>()).Take(4).ToList(),
                    usedFallback = false
                },
                products,
                candidateCount = catalog.Count,
                error = (string?)null
            });
        }
        catch
        {
            var fallbackReply = "Mình chưa soi được ảnh này, bạn thử ảnh rõ hơn hoặc chụp gần món đồ hơn nha.";
            return Json(new
            {
                ok = true,
                type = "chat",
                userMessage = userDescription,
                assistantMessage = fallbackReply,
                error = (string?)null
            });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ClearChatHistory()
    {
        HttpContext.Session.Remove(AiStylistChatHistorySessionKey);
        HttpContext.Session.Remove(AiStylistLastResultSessionKey);
        return Json(new { ok = true, message = _localizer.Get("AiStylist:ClearHistorySuccess") });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProductAdvisor([FromBody] ProductAdvisorRequest? request, CancellationToken cancellationToken)
    {
        if (request == null)
            return Json(new { ok = false, message = "Không đọc được yêu cầu tư vấn. Bạn thử lại giúp mình nhé." });

        if (request.ProductId <= 0)
            return Json(new { ok = false, message = _localizer.Get("AiStylist:InvalidProduct") });

        if (!string.IsNullOrWhiteSpace(request.NeedDescription) && request.NeedDescription.Length > 800)
            return Json(new { ok = false, message = "Nhu cầu đang hơi dài, bạn rút gọn còn khoảng 800 ký tự giúp mình nhé." });

        var product = await _context.Products
            .AsNoTracking()
            .Where(p => p.Id == request.ProductId && !p.Discontinued)
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Images)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Images)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(cancellationToken);

        if (product == null)
            return Json(new { ok = false, message = _localizer.Get("AiStylist:CartProductNotFound") });

        var alternatives = await LoadAdvisorAlternativesAsync(product, request, cancellationToken);
        var result = _aiProvider.IsConfigured
            ? await BuildAiProductAdvisorResultAsync(product, request, alternatives, cancellationToken)
            : BuildFallbackProductAdvisorResult(product, request, alternatives);

        return ProductAdvisorJson(result);
    }

    private IActionResult ProductAdvisorJson(ProductAdvisorResult result)
        => Json(new
        {
            ok = true,
            result = new
            {
                verdict = result.Verdict,
                verdictLabel = result.VerdictLabel,
                confidence = result.Confidence,
                summary = result.Summary,
                fitNotes = result.FitNotes,
                watchOuts = result.WatchOuts,
                usedFallback = result.UsedFallback
            },
            betterMatches = result.BetterMatches.Select(p => new
            {
                productId = p.ProductId,
                productName = p.ProductName,
                categoryName = p.CategoryName,
                unitPrice = p.UnitPrice,
                stockQuantity = p.StockQuantity,
                imageUrl = p.ImageUrl,
                reason = p.Reason,
                rank = p.Rank,
                hasRoom3DModel = p.HasRoom3DModel
            })
        });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProductImageAdvisor(
        [FromForm] int productId,
        [FromForm] string? needDescription,
        [FromForm] decimal? budget,
        [FromForm] string? roomType,
        [FromForm] IFormFile? image,
        CancellationToken cancellationToken)
    {
        if (!_aiProvider.IsConfigured)
            return Json(new { ok = false, message = "AI chưa sẵn sàng để soi ảnh lúc này." });

        if (image == null)
            return Json(new { ok = false, message = "Bạn gửi thêm ảnh món đồ để mình soi nhé." });

        var imageValidation = ValidateAdvisorImage(image);
        if (imageValidation != null)
            return Json(new { ok = false, message = imageValidation });

        var currentProduct = await LoadAdvisorProductAsync(productId, cancellationToken);
        if (currentProduct == null)
            return Json(new { ok = false, message = _localizer.Get("AiStylist:CartProductNotFound") });

        var request = new ProductAdvisorRequest
        {
            ProductId = productId,
            NeedDescription = needDescription ?? string.Empty,
            Budget = budget,
            RoomType = roomType ?? string.Empty
        };

        var candidateFilter = BuildImageCandidateFilter(
            string.Join(" ", needDescription, currentProduct.ProductName, currentProduct.Category?.CategoryName, currentProduct.QuantityPerUnit),
            roomType,
            null,
            budget,
            null,
            null,
            null);
        var catalog = await LoadImageAdvisorCatalogAsync(currentProduct, candidateFilter, cancellationToken);

        await using var stream = image.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);

        try
        {
            var prompt = BuildProductImageAdvisorPrompt(currentProduct, request, catalog);
            var raw = await _aiProvider.GenerateJsonWithImageAsync(prompt, memory.ToArray(), image.ContentType, cancellationToken);
            using var document = JsonDocument.Parse(ExtractJson(raw));
            var root = document.RootElement;

            var matchReasons = GetImageMatchReasons(root);
            var matchingIds = GetJsonIntArray(root, "matchingProductIds")
                .Where(id => catalog.Any(product => product.Id == id))
                .ToList();
            var matches = matchingIds
                .Select(id => catalog.FirstOrDefault(product => product.Id == id))
                .Where(product => product != null)
                .Cast<Product>()
                .Take(4)
                .Select((product, index) => ToAdvisorProductViewModel(
                    new AdvisorScoredProduct(product, 0, matchReasons.GetValueOrDefault(product.Id, "Gần với ảnh bạn gửi.")),
                    index + 1))
                .ToList();

            return Json(new
            {
                ok = true,
                matchTitle = "Sản phẩm gần ảnh nhất",
                result = new
                {
                    verdict = "consider",
                    verdictLabel = GetJsonString(root, "headline", "Đã soi ảnh xong"),
                    confidence = Math.Clamp(GetJsonInt(root, "confidence", matches.Any() ? 74 : 55), 45, 95),
                    summary = SanitizeAdvisorText(GetJsonString(root, "summary", "Mình đã nhận diện ảnh và gợi ý các món gần vibe nhất trong catalog.")),
                    fitNotes = GetJsonStringArray(root, "visualNotes", Array.Empty<string>()).Select(SanitizeAdvisorText).ToList(),
                    watchOuts = GetJsonStringArray(root, "watchOuts", Array.Empty<string>()).Select(SanitizeAdvisorText).ToList(),
                    usedFallback = false
                },
                betterMatches = matches
            });
        }
        catch
        {
            return Json(new { ok = false, message = "AI chưa soi được ảnh này, bạn thử ảnh rõ hơn giúp mình nhé." });
        }
    }

    private async Task<Product?> LoadAdvisorProductAsync(int productId, CancellationToken cancellationToken)
        => await _context.Products
            .AsNoTracking()
            .Where(p => p.Id == productId && !p.Discontinued)
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Images)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Images)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(cancellationToken);

    private async Task<List<AdvisorScoredProduct>> LoadAdvisorAlternativesAsync(Product currentProduct, ProductAdvisorRequest request, CancellationToken cancellationToken)
    {
        var currentCategory = currentProduct.Category?.CategoryName ?? string.Empty;
        var filter = new AiCandidateFilter
        {
            UserDescription = string.Join(" ", request.NeedDescription, request.RoomType, currentProduct.ProductName, currentCategory, currentProduct.QuantityPerUnit),
            RoomType = request.RoomType,
            Budget = request.Budget,
            MaxCandidates = 60
        };
        var candidateIds = (await _candidateService.GetCandidatesAsync(filter, cancellationToken))
            .Select(candidate => candidate.ProductId)
            .Where(id => id != currentProduct.Id)
            .Distinct()
            .Take(60)
            .ToList();

        if (!candidateIds.Any())
            return new List<AdvisorScoredProduct>();

        var rank = candidateIds
            .Select((id, index) => new { id, index })
            .ToDictionary(item => item.id, item => item.index);

        var candidates = await _context.Products
            .AsNoTracking()
            .Where(p => candidateIds.Contains(p.Id) && !p.Discontinued)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Images)
            .Include(p => p.Reviews)
            .ToListAsync(cancellationToken);

        var currentScore = GetAdvisorScore(currentProduct, request);
        return candidates
            .Select(product =>
            {
                var score = GetAdvisorScore(product, request);
                if (product.CategoryID == currentProduct.CategoryID)
                    score += 7;
                if (IsAdvisorComplement(currentCategory, product.Category?.CategoryName ?? string.Empty))
                    score += 5;

                return new AdvisorScoredProduct(product, score, BuildAdvisorAlternativeReason(product, request, currentScore, score));
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => rank.GetValueOrDefault(item.Product.Id, int.MaxValue))
            .ThenByDescending(item => GetProductStock(item.Product))
            .ThenBy(item => item.Product.ProductName)
            .Take(8)
            .ToList();
    }

    private static AiCandidateFilter BuildImageCandidateFilter(
        string? userDescription,
        string? roomType,
        string? style,
        decimal? budget,
        string? priority,
        string? preferredColors,
        string? avoidColors)
        => new()
        {
            UserDescription = userDescription,
            RoomType = roomType,
            Style = style,
            Budget = budget,
            Priority = priority,
            PreferredColors = SplitFilterList(preferredColors),
            AvoidColors = SplitFilterList(avoidColors),
            MaxCandidates = 300
        };

    private async Task<List<Product>> LoadImageAdvisorCatalogAsync(Product? currentProduct, AiCandidateFilter filter, CancellationToken cancellationToken)
    {
        var candidates = await _candidateService.GetCandidatesAsync(filter, cancellationToken);
        var ids = candidates
            .Select(candidate => candidate.ProductId)
            .Distinct()
            .ToList();

        if (currentProduct != null && !ids.Contains(currentProduct.Id))
            ids.Insert(0, currentProduct.Id);

        if (!ids.Any())
            return new List<Product>();

        var cacheKey = $"{AiImageCatalogDetailsCachePrefix}{string.Join("-", ids.OrderBy(id => id))}";
        var products = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            entry.SetPriority(CacheItemPriority.Normal);

            return await _context.Products
                .AsNoTracking()
                .Where(p => ids.Contains(p.Id) && !p.Discontinued)
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Images)
                .Include(p => p.Reviews)
                .ToListAsync(cancellationToken);
        }) ?? new List<Product>();

        var rank = ids.Select((id, index) => new { id, index })
            .ToDictionary(item => item.id, item => item.index);

        return products
            .OrderBy(product => rank.GetValueOrDefault(product.Id, int.MaxValue))
            .ThenBy(product => product.ProductName)
            .ToList();
    }

    private static IReadOnlyList<string> SplitFilterList(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value
                .Split(new[] { ',', ';', '/', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(item => item.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

    private static string? ValidateAdvisorImage(IFormFile image)
    {
        const long maxSize = 5 * 1024 * 1024;
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        if (image.Length <= 0 || image.Length > maxSize)
            return "Ảnh nên nhỏ hơn 5MB để AI soi cho mượt nha.";

        if (!allowed.Contains(image.ContentType))
            return "Ảnh chỉ hỗ trợ JPG, PNG hoặc WEBP nha.";

        return null;
    }

    private async Task<ProductAdvisorResult> BuildAiProductAdvisorResultAsync(
        Product product,
        ProductAdvisorRequest request,
        IReadOnlyList<AdvisorScoredProduct> alternatives,
        CancellationToken cancellationToken)
    {
        var fallback = BuildFallbackProductAdvisorResult(product, request, alternatives);

        try
        {
            var prompt = BuildProductAdvisorPrompt(product, request, alternatives);
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(ProductAdvisorAiTimeout);
            var raw = await _aiProvider.GenerateJsonAsync(prompt, timeout.Token);
            using var document = JsonDocument.Parse(ExtractJson(raw));
            var root = document.RootElement;

            var betterIds = root.TryGetProperty("betterProductIds", out var idArray) && idArray.ValueKind == JsonValueKind.Array
                ? idArray.EnumerateArray()
                    .Where(item => item.ValueKind == JsonValueKind.Number)
                    .Select(item => item.GetInt32())
                    .ToList()
                : new List<int>();
            var currentScore = GetAdvisorScore(product, request);

            var betterMatches = betterIds
                .Select(id => alternatives.FirstOrDefault(item => item.Product.Id == id))
                .Where(item => item != null)
                .Cast<AdvisorScoredProduct>()
                .Where(item => IsMeaningfullyBetterAlternative(currentScore, product, item, request))
                .Take(3)
                .Select((item, index) => ToAdvisorProductViewModel(item, index + 1))
                .ToList();

            var aiVerdict = NormalizeAdvisorVerdict(GetJsonString(root, "verdict", fallback.Verdict), fallback.Verdict);
            var verdict = ResolveAdvisorVerdict(fallback.Verdict, aiVerdict, betterMatches.Any());
            var summary = verdict == aiVerdict
                ? SanitizeAdvisorText(GetJsonString(root, "summary", fallback.Summary))
                : fallback.Summary;
            var serverConfidence = BuildAdvisorFitPercent(currentScore, verdict, product, request, betterMatches.Any());
            var aiConfidence = GetJsonInt(root, "confidence", fallback.Confidence);

            return new ProductAdvisorResult
            {
                Verdict = verdict,
                VerdictLabel = GetAdvisorVerdictLabel(verdict),
                Confidence = BlendAdvisorConfidence(aiConfidence, serverConfidence, verdict),
                Summary = summary,
                FitNotes = GetJsonStringArray(root, "fitNotes", fallback.FitNotes).Select(SanitizeAdvisorText).ToList(),
                WatchOuts = GetJsonStringArray(root, "watchOuts", fallback.WatchOuts).Select(SanitizeAdvisorText).ToList(),
                BetterMatches = betterMatches,
                UsedFallback = false
            };
        }
        catch
        {
            return fallback;
        }
    }

    private ProductAdvisorResult BuildFallbackProductAdvisorResult(Product product, ProductAdvisorRequest request, IReadOnlyList<AdvisorScoredProduct> alternatives)
    {
        var currentScore = GetAdvisorScore(product, request);
        var stock = GetProductStock(product);
        var betterMatches = alternatives
            .Where(item => IsMeaningfullyBetterAlternative(currentScore, product, item, request))
            .Take(3)
            .Select((item, index) => ToAdvisorProductViewModel(item, index + 1))
            .ToList();

        var budget = request.Budget.GetValueOrDefault();
        var overBudget = budget > 0 && product.UnitPrice > budget;
        var hasNeed = !string.IsNullOrWhiteSpace(request.NeedDescription) || !string.IsNullOrWhiteSpace(request.RoomType) || budget > 0;
        var verdict = overBudget || stock <= 0
            ? "skip"
            : currentScore >= 24 && !betterMatches.Any()
                ? "buy"
                : "consider";

        var notes = new List<string>
        {
            $"Thuộc nhóm {product.Category?.CategoryName ?? "nội thất"}, dễ đưa vào bản phối nếu đúng không gian.",
            stock > 0 ? $"Tồn kho đang có {stock:N0}, đủ an toàn để cân nhắc đặt." : "Tồn kho hiện chưa ổn cho lựa chọn này.",
            budget > 0 && product.UnitPrice <= budget
                ? $"Giá {product.UnitPrice:N0} VND nằm trong ngân sách {budget:N0} VND."
                : budget > 0
                    ? $"Giá {product.UnitPrice:N0} VND đang vượt ngân sách {budget:N0} VND."
                    : "Bạn chưa nhập ngân sách, nên mình đánh giá theo độ phù hợp tổng quan."
        };

        var watchOuts = new List<string>();
        if (!hasNeed)
            watchOuts.Add("Bạn chưa nói rõ phòng, diện tích hoặc mục tiêu sử dụng nên mức tư vấn vẫn ở dạng nhanh.");
        if (overBudget)
            watchOuts.Add("Sản phẩm đang vượt ngân sách bạn nhập, nên chưa phải lựa chọn tối ưu.");
        if (!product.Reviews.Any(r => r.IsApproved))
            watchOuts.Add("Sản phẩm chưa có nhiều tín hiệu review để đối chiếu trải nghiệm thực tế.");
        if (betterMatches.Any())
            watchOuts.Add("Có vài sản phẩm khác trong catalog đang khớp nhu cầu rõ hơn, nên đáng so thêm trước khi chốt.");

        return new ProductAdvisorResult
        {
            Verdict = verdict,
            VerdictLabel = GetAdvisorVerdictLabel(verdict),
            Confidence = BuildAdvisorFitPercent(currentScore, verdict, product, request, betterMatches.Any()),
            Summary = verdict switch
            {
                "buy" => $"Ê nghe nè, mình nghiêng về chốt {product.ProductName} nếu đúng gu bạn đang tìm. Nó đang cân bằng ổn giữa nhu cầu, ngân sách, tồn kho và độ hợp vibe, ví chưa khóc đâu.",
                "skip" => $"Khoan chốt đã, {product.ProductName} đang có tín hiệu chưa khớp lắm. Đẹp thì có thể đẹp, nhưng mua hàng là phải tỉnh, nhất là khi vướng ngân sách hoặc tồn kho.",
                _ => $"Soi nhanh phát: {product.ProductName} đáng để ngó, nhưng mình chưa muốn bạn bấm mua trong vô thức. Nó ổn, chỉ cần so thêm vài điểm còn thiếu trước khi chốt."
            },
            FitNotes = notes,
            WatchOuts = watchOuts,
            BetterMatches = betterMatches,
            UsedFallback = true
        };
    }

    private static string BuildProductAdvisorPrompt(Product product, ProductAdvisorRequest request, IReadOnlyList<AdvisorScoredProduct> alternatives)
    {
        var payload = new
        {
            userNeed = new
            {
                request.NeedDescription,
                request.Budget,
                request.RoomType
            },
            currentProduct = ToAdvisorPromptProduct(product, "current"),
            alternatives = alternatives.Take(6).Select(item => new
            {
                product = ToAdvisorPromptProduct(item.Product, item.Reason),
                scoreHint = Math.Round(item.Score, 2)
            }),
            currentScoreHint = Math.Round(GetAdvisorScore(product, request), 2)
        };

        return $$"""
You are an experienced Vietnamese furniture buyer and interior consultant for a real ecommerce website.
You have years of showroom/ecommerce advising experience, so judge like someone who has compared many products, variants, prices, stock levels, and customer needs.
Your goal is NOT to sell at all costs. Your goal is to help the customer decide whether the current product is a sensible purchase for their stated need.
Your voice is playful, friendly Vietnamese bestie energy: nhí nhảnh, hơi lầy, thân mật như đang tư vấn cho bạn thân đi shopping, but suitable for many ages and genders.
Make the answer noticeably lively, not just mildly friendly. It should feel like a fun friend quickly roasting/checking the product before the user spends money.
Prefer playful openings like "Ê nghe nè", "Nói thiệt nha", "Chốt hạ nè", "Khoan chốt đã", "Soi nhanh phát", "Ok để mình soi", "Ui cái này", or "Rồi để tui phán nhẹ".
You may use "chào con vk" when the user is clearly joking/casual or asks for that vibe, because it reads as playful slang here.
Avoid defaulting to gendered address words like "bà", "chị", "anh", or "em gái" unless the user uses that address first.
Use light teasing and cute phrases such as "chốt đơn", "chất luôn", "ngầu đét", "rén ví", "đáng suy nghĩ", "ví chưa khóc", "đẹp nhưng phải tỉnh", "đáng để ngó", "đừng bấm mua trong vô thức", but keep it tasteful and not insulting.
Each summary should contain at least one playful phrase, but every sentence must still carry useful buying advice.
Do NOT become a dry corporate report. Still be honest and practical.

Think like a buyer advisor using this rubric before writing JSON:
1. Need fit (0-30): Does the product type/category match the user's room, use case, style, and constraints?
   Product category can be broad or imperfect, so also use product name, description, price, and visual intent. Do not over-penalize a decor product just because its category is a room name.
2. Budget fit (0-20): The budget is VND. If price <= budget, it is affordable. A very high budget means more flexibility, not a mismatch. Penalize only if price > budget or value seems weak compared with alternatives.
3. Space/practicality fit (0-15): If the user mentions room size or use context, judge whether the product sounds practical. If dimensions are unavailable, say so instead of inventing measurements.
4. Availability/reliability (0-15): Stock > 0 is positive. Reviews/ratings are useful but absence of reviews is only a caution, not automatic rejection.
5. Variant fit (0-10): Use variants/colors/sizes/SKU/finalPrice to judge whether the product has a suitable option for the user's taste and budget. Mention useful variant choices when relevant.
6. Alternatives comparison (0-20): Compare against the provided alternatives. Recommend alternatives only if they are clearly better for the stated need, budget, category fit, stock, variant availability, or value.

Decision rules:
- verdict "buy": current product strongly matches the need, is within budget or budget is not an issue, and alternatives are not clearly better.
- verdict "consider": current product is acceptable but missing information, has moderate fit, or alternatives may be worth comparing.
- verdict "skip": current product is over budget, out of stock, wrong category/use case, or a provided alternative is clearly a better fit.
- Do not mark "skip" just because the customer has a large budget.
- If the user says they like the current product, likes "decor", "ngầu", "cá tính", "độc", "lạ", "ấn tượng", or wants a statement piece, treat visual uniqueness as a strong positive signal.
- For statement decor requests, do NOT replace a bold/unique product with safer cheaper items only because they are cheaper or more practical.
- Do not infer poor quality from a low price unless the catalog explicitly provides quality issues. If the price looks unusually low, mention it as "nên kiểm tra lại giá niêm yết/thông tin sản phẩm", not as proof that the product is bad.
- Never say or imply that low price means bad material, bad quality, "hàng hệ tâm linh", or a risky product. Low price is only a data-check caution.
- Do not claim the product is reliable, premium, durable, authentic, lucky, feng-shui friendly, or high quality unless the input data explicitly supports it.
- Do not invent dimensions, materials, finishes, delivery promises, warranty, or room-specific effects. If useful data is missing, say it is missing in a friendly way.
- Avoid absolute guarantee language like "Ä‘áº£m báº£o", "cháº¯c cháº¯n", or "khÃ´ng cáº§n suy nghÄ©" unless it is about a factual field in the input. Even with verdict "buy", still sound like a careful advisor.
- Do not praise blindly. Mention real cautions when data is missing.
- Recommend only product IDs from `alternatives`.
- `betterProductIds` should include 0-3 IDs only if they beat the current product for the SAME user intent. If the current product is already a strong fit, return [].
- Cheaper is only a meaningful advantage when the current product is over budget or the user explicitly asks to save money.
- You are the primary judge for `confidence`: compare the current product against alternatives, variants, budget, stock, reviews, and user intent before assigning it.
- Treat `currentScoreHint` and `scoreHint` as helper signals only, not as blind truth. If your experienced judgment disagrees, explain through verdict/reasons and choose the confidence that best represents fit.
- `confidence` is the displayed fit percentage. Keep it stable and practical; do not swing wildly for small wording changes when product, budget, and intent are essentially the same. Use 45-95 only:
  - 85-95: strong evidence and strong fit.
  - 70-84: good fit but some unknowns.
  - 55-69: mixed fit or missing important info.
  - 45-54: weak fit but still enough data to advise.
- Return valid JSON only. No markdown. No extra text.
- Vietnamese copy should be concise, specific, playful, and directly tied to the product/user need. Avoid generic sales language.
- `summary`, `fitNotes`, and `watchOuts` should sound like a fun friend advising honestly, not a formal product audit.
- Keep the humor short and punchy. Do not ramble. Do not use emojis unless the user's tone is extremely casual.

JSON schema:
{
  "verdict": "buy|consider|skip",
  "verdictLabel": "Nên mua|Nên cân nhắc|Chưa nên mua",
  "confidence": 78,
  "summary": "2-3 playful Vietnamese sentences explaining the practical decision",
  "fitNotes": ["playful but specific reason this product fits the stated need"],
  "watchOuts": ["playful but specific caution or missing info"],
  "betterProductIds": [1, 2]
}

Input:
{{JsonSerializer.Serialize(payload)}}
""";
    }

    private static string BuildProductImageAdvisorPrompt(Product? currentProduct, ProductAdvisorRequest request, IReadOnlyList<Product> catalog, object? extraPreferences = null)
    {
        var payload = new
        {
            userNeed = new
            {
                request.NeedDescription,
                request.Budget,
                request.RoomType,
                extraPreferences
            },
            currentProduct = currentProduct == null ? null : ToAdvisorPromptProduct(currentProduct, "current detail page product"),
            catalog = catalog.Select(product => ToAdvisorPromptProduct(product, currentProduct != null && product.Id == currentProduct.Id ? "current product" : "catalog candidate"))
        };

        return $$"""
Bạn là AI tư vấn tìm sản phẩm nội thất bằng ảnh cho website thương mại điện tử.
Người dùng đã gửi một ảnh món đồ họ thích. Trước hết hãy nhìn ảnh để nhận diện loại món, dáng, màu, chất liệu nhìn thấy được, phong cách và bối cảnh sử dụng có thể suy ra.
Sau đó so ảnh với dữ liệu catalog được cung cấp. Không được bịa chi tiết catalog không có; chỉ dùng tên sản phẩm, danh mục, mô tả, biến thể, giá, tồn kho, đánh giá và tín hiệu số ảnh.
Mục tiêu là tìm các sản phẩm trong catalog giống ảnh hoặc gần vibe ảnh nhất, rồi phản hồi trung thực bằng tiếng Việt có dấu.

Quy tắc:
- Tất cả nội dung trả lời cho người dùng phải là tiếng Việt có dấu tự nhiên. Không trả tiếng Việt không dấu.
- Giọng vui, lanh lợi, hơi lầy nhẹ như bạn thân biết nội thất, nhưng vẫn thực tế và không hứa quá tay.
- Ảnh có thể là ảnh nhân vật, biểu tượng, anime/game, linh vật, tranh, poster, tượng, mô hình hoặc đồ sưu tầm. Đừng loại ngay chỉ vì nó không phải ảnh nội thất truyền thống; hãy tìm sản phẩm decor/statue/sculpture/art/poster/figure trong catalog có chủ đề, tên gọi hoặc vibe tương ứng.
- Nếu ảnh là nhân vật/chủ đề nổi tiếng như rồng, Dragon Ball, Shenron, anime, game, kiếm, robot, thú mascot..., hãy ưu tiên match theo chủ đề và tên sản phẩm trước, rồi mới xét dáng/màu/style.
- Chỉ nói "không tìm được" khi catalog thật sự không có sản phẩm nào liên quan sau khi đã kiểm tra tên, mô tả, category và vibe. Nếu có sản phẩm liên quan theo chủ đề thì phải trả ID đó, dù ảnh gốc là minh họa/hoạt hình.
- Nếu ảnh quá mờ, không nhìn ra chủ thể chính, hoặc không có chủ đề nào khớp catalog, hãy nói rõ và trả ít ID hơn.
- Chỉ trả productId có trong `catalog`.
- `matchingProductIds` trả 0-4 ID, sắp từ giống/gần nhất đến yếu hơn.
- Ưu tiên cùng loại món trước, sau đó mới xét style, màu và mục đích dùng. Đừng chọn món rẻ hơn chỉ vì nó rẻ.
- Nếu chỉ giống vibe/style chứ không giống cùng loại món, phải nói rõ.
- Không bịa kích thước, chất liệu, thương hiệu, chất lượng, bảo hành hoặc giao hàng.
- `confidence` là độ tự tin khớp ảnh, 45-95:
  - 85-95: cùng loại món và khớp mạnh về dáng/màu/style.
  - 70-84: khá liên quan nhưng còn khác chi tiết hoặc catalog thiếu dữ liệu.
  - 55-69: chỉ gần vibe hoặc gần danh mục.
  - 45-54: khớp yếu hoặc ảnh chưa rõ.
- Trả JSON hợp lệ. Không markdown. Không thêm chữ ngoài JSON.

JSON schema:
{
  "headline": "Đã tìm thấy món gần vibe",
  "confidence": 78,
  "summary": "2-3 câu tiếng Việt có dấu, vui nhưng thực tế về ảnh và gợi ý",
  "visualNotes": ["Đặc điểm AI thấy trong ảnh và đã dùng để tìm"],
  "watchOuts": ["Cảnh báo nếu dữ liệu thiếu hoặc ảnh chưa rõ"],
  "matchingProductIds": [1, 2],
  "matches": [
    { "productId": 1, "reason": "Lý do ngắn gọn vì sao món này gần ảnh" }
  ]
}

Catalog input:
{{JsonSerializer.Serialize(payload)}}
""";
    }

    private static object ToAdvisorPromptProduct(Product product, string reason)
    {
        var activeVariants = product.Variants?
            .Where(v => v.IsActive)
            .OrderBy(v => v.Color)
            .ThenBy(v => v.Size)
            .ToList() ?? new List<ProductVariant>();
        var variantPrices = activeVariants.Any()
            ? activeVariants.Select(v => product.UnitPrice + (v.PriceAdjustment ?? 0)).ToList()
            : new List<decimal> { product.UnitPrice };
        var colors = activeVariants
            .Select(v => v.Color)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var sizes = activeVariants
            .Select(v => v.Size)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new
        {
            productId = product.Id,
            name = product.ProductName,
            category = product.Category?.CategoryName ?? "Other",
            visualSearchKeywords = BuildVisualSearchKeywords(product),
            supplier = product.Supplier?.CompanyName,
            price = product.UnitPrice,
            mainImageUrl = product.Images?
                .OrderByDescending(i => i.IsMain)
                .ThenBy(i => i.DisplayOrder)
                .FirstOrDefault()
                ?.ImageUrl,
            priceRange = new
            {
                min = variantPrices.Min(),
                max = variantPrices.Max()
            },
            description = product.QuantityPerUnit,
            stock = GetProductStock(product),
            colors,
            sizes,
            variants = activeVariants.Take(12).Select(v => new
            {
                variantId = v.Id,
                color = v.Color,
                size = v.Size,
                sku = v.SKU,
                stock = v.StockQuantity,
                priceAdjustment = v.PriceAdjustment ?? 0,
                finalPrice = product.UnitPrice + (v.PriceAdjustment ?? 0),
                imageCount = v.Images?.Count ?? 0
            }),
            averageRating = GetAverageRating(product),
            reviewCount = product.Reviews?.Count(r => r.IsApproved) ?? 0,
            hasRoom3DModel = Room3DProductCatalog.TryGetRoomProductId(product, out _),
            reason
        };
    }

    private static IReadOnlyList<string> BuildVisualSearchKeywords(Product product)
    {
        var text = string.Join(" ", product.ProductName, product.Category?.CategoryName, product.QuantityPerUnit).ToLowerInvariant();
        var keywords = new List<string>();

        void AddIf(bool condition, params string[] values)
        {
            if (condition)
                keywords.AddRange(values);
        }

        AddIf(ContainsAny(text, "shenron", "dragon", "rồng"), "dragon", "rồng", "Shenron", "Dragon Ball", "anime", "manga", "decor", "statue", "sculpture", "figure", "statement piece");
        AddIf(ContainsAny(text, "katana", "samurai", "kiếm"), "katana", "samurai", "anime", "wall decor", "statement piece");
        AddIf(ContainsAny(text, "tapestry", "wall hanging", "canvas", "art"), "wall art", "poster", "canvas", "tapestry", "decor");
        AddIf(ContainsAny(text, "vase", "bình"), "vase", "decor", "table decor");
        AddIf(ContainsAny(text, "lamp", "sconce", "đèn"), "lamp", "lighting", "decor lighting");

        return keywords
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(16)
            .ToList();
    }

    private static AiRecommendedProductViewModel ToAdvisorProductViewModel(AdvisorScoredProduct item, int rank)
        => new()
        {
            ProductId = item.Product.Id,
            ProductName = item.Product.ProductName,
            CategoryName = item.Product.Category?.CategoryName ?? "Other",
            UnitPrice = item.Product.UnitPrice,
            StockQuantity = GetProductStock(item.Product),
            ImageUrl = item.Product.Images
                .OrderByDescending(i => i.IsMain)
                .ThenBy(i => i.DisplayOrder)
                .FirstOrDefault()
                ?.ImageUrl,
            Reason = item.Reason,
            Rank = rank,
            HasRoom3DModel = Room3DProductCatalog.TryGetRoomProductId(item.Product, out _)
        };

    private static double GetAdvisorScore(Product product, ProductAdvisorRequest request)
    {
        var text = string.Join(" ", product.ProductName, product.Category?.CategoryName, product.QuantityPerUnit).ToLowerInvariant();
        var needText = string.Join(" ", request.NeedDescription, request.RoomType).ToLowerInvariant();
        var needTokens = Tokenize(needText);
        var stock = GetProductStock(product);
        var score = 0d;

        score += product.Images.Any(i => !string.IsNullOrWhiteSpace(i.ImageUrl)) ? 4 : 0;
        score += Math.Min(stock, 20) * 0.35;
        score += GetAverageRating(product) * 1.2;
        score += needTokens.Count(token => text.Contains(token)) * 4;
        score += GetStyleIntentScore(text, needText);

        if (request.Budget.HasValue && request.Budget > 0)
        {
            if (product.UnitPrice <= request.Budget.Value)
                score += 9;
            else
                score -= Math.Min(14, (double)((product.UnitPrice - request.Budget.Value) / Math.Max(request.Budget.Value, 1) * 12));
        }

        return score;
    }

    private static double GetStyleIntentScore(string productText, string needText)
    {
        var score = 0d;
        var asksForDecor = ContainsAny(needText, "decor", "trang trí", "đồ trang trí", "điểm nhấn");
        var asksForStatement = ContainsAny(needText, "ngầu", "cá tính", "độc", "lạ", "ấn tượng", "nổi bật", "statement", "cool");

        if (asksForDecor && ContainsAny(productText, "decor", "trang trí", "dragon", "shenron", "guardian", "art", "canvas", "vase", "sculpture", "statue", "tapestry", "candle"))
            score += 8;

        if (asksForStatement && ContainsAny(productText, "dragon", "shenron", "guardian", "sculpture", "statue", "abstract", "katana", "art"))
            score += 14;

        if (asksForStatement && ContainsAny(productText, "sconce", "lamp", "tapestry", "wall hanging") && !ContainsAny(productText, "dragon", "sculpture", "statue", "abstract"))
            score -= 4;

        return score;
    }

    private static string BuildAdvisorAlternativeReason(Product product, ProductAdvisorRequest request, double currentScore, double alternativeScore)
    {
        if (request.Budget.HasValue && request.Budget > 0 && product.UnitPrice <= request.Budget.Value && alternativeScore > currentScore + 10)
            return $"Có điểm phù hợp nhu cầu cao hơn trong tầm ngân sách, giá khoảng {product.UnitPrice:N0} VND.";

        return $"Có độ phù hợp tốt với nhu cầu đã nhập và đang còn {GetProductStock(product):N0} sản phẩm.";
    }

    private static bool IsMeaningfullyBetterAlternative(double currentScore, Product currentProduct, AdvisorScoredProduct alternative, ProductAdvisorRequest request)
    {
        var budget = request.Budget.GetValueOrDefault();
        var currentOverBudget = budget > 0 && currentProduct.UnitPrice > budget;
        var alternativeWithinBudget = budget > 0 && alternative.Product.UnitPrice <= budget;
        var asksToSave = ContainsAny((request.NeedDescription ?? string.Empty).ToLowerInvariant(), "rẻ", "tiết kiệm", "giá tốt", "budget", "ngân sách thấp");

        if (alternative.Score < currentScore + 10)
            return false;

        if (!currentOverBudget && alternative.Product.UnitPrice < currentProduct.UnitPrice && !asksToSave)
        {
            var scoreGap = alternative.Score - currentScore;
            return scoreGap >= 16;
        }

        return currentOverBudget ? alternativeWithinBudget : true;
    }

    private static bool IsAdvisorComplement(string currentCategory, string candidateCategory)
    {
        var current = currentCategory.ToLowerInvariant();
        var candidate = candidateCategory.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(current) || string.IsNullOrWhiteSpace(candidate) || current == candidate)
            return false;

        return ContainsAny(current, "sofa", "ghế", "chair", "bàn", "table", "giường", "bed")
            && ContainsAny(candidate, "đèn", "lamp", "decor", "trang trí", "kệ", "shelf", "gối", "pillow", "thảm", "rug");
    }

    private static int GetProductStock(Product product)
    {
        var activeVariants = product.Variants?.Where(v => v.IsActive).ToList() ?? new List<ProductVariant>();
        return activeVariants.Any() ? activeVariants.Sum(v => Math.Max(v.StockQuantity, 0)) : 100;
    }

    private static double GetAverageRating(Product product)
    {
        var approved = product.Reviews?.Where(r => r.IsApproved).ToList() ?? new List<Review>();
        return approved.Any() ? approved.Average(r => r.Rating) : 0;
    }

    private static IReadOnlyList<string> Tokenize(string value)
        => value
            .ToLowerInvariant()
            .Replace("_", " ")
            .Replace("-", " ")
            .Split(new[] { ' ', ',', ';', '/', '|', '.', ':', '(', ')' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(token => token.Length >= 2)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static string GetJsonString(JsonElement root, string propertyName, string fallback)
        => root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? fallback
            : fallback;

    private static int GetJsonInt(JsonElement root, string propertyName, int fallback)
        => root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number)
            ? number
            : fallback;

    private static IReadOnlyList<int> GetJsonIntArray(JsonElement root, string propertyName)
        => root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.Number)
                .Select(item => item.GetInt32())
                .Distinct()
                .Take(8)
                .ToList()
            : Array.Empty<int>();

    private static Dictionary<int, string> GetImageMatchReasons(JsonElement root)
    {
        if (!root.TryGetProperty("matches", out var matches) || matches.ValueKind != JsonValueKind.Array)
            return new Dictionary<int, string>();

        return matches.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.Object
                && item.TryGetProperty("productId", out var id)
                && id.ValueKind == JsonValueKind.Number
                && id.TryGetInt32(out _))
            .Select(item =>
            {
                var id = item.GetProperty("productId").GetInt32();
                var reason = item.TryGetProperty("reason", out var reasonElement) && reasonElement.ValueKind == JsonValueKind.String
                    ? reasonElement.GetString() ?? string.Empty
                    : string.Empty;

                return new KeyValuePair<int, string>(id, reason);
            })
            .GroupBy(item => item.Key)
            .ToDictionary(group => group.Key, group => string.IsNullOrWhiteSpace(group.First().Value) ? "Gần với ảnh bạn gửi." : group.First().Value);
    }

    private static string NormalizeAdvisorVerdict(string verdict, string fallback)
        => verdict.Trim().ToLowerInvariant() switch
        {
            "buy" => "buy",
            "consider" => "consider",
            "skip" => "skip",
            _ => fallback
        };

    private static string GetAdvisorVerdictLabel(string verdict)
        => verdict switch
        {
            "buy" => "Nên mua",
            "skip" => "Chưa nên mua",
            _ => "Nên cân nhắc"
        };

    private static string ResolveAdvisorVerdict(string serverVerdict, string aiVerdict, bool hasBetterMatch)
    {
        if (serverVerdict == "skip")
            return "skip";

        if (serverVerdict == "buy" && !hasBetterMatch)
            return "buy";

        return aiVerdict;
    }

    private static string SanitizeAdvisorText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        var lower = value.ToLowerInvariant();
        var linksLowPriceToQuality = ContainsAny(lower, "giá rẻ bất ngờ", "giá quá rẻ", "rẻ bất ngờ")
            && ContainsAny(lower, "chất liệu", "chất lượng", "hàng hệ tâm linh", "hú hồn", "rủi ro");

        return linksLowPriceToQuality
            ? "Giá niêm yết có vẻ hơi bất thường, nên kiểm tra lại thông tin giá/sản phẩm trước khi chốt cho chắc nha."
            : value;
    }

    private static int BlendAdvisorConfidence(int aiConfidence, int serverConfidence, string verdict)
    {
        var guardedServer = verdict switch
        {
            "buy" => Math.Clamp(serverConfidence, 70, 95),
            "skip" => Math.Clamp(serverConfidence, 35, 62),
            _ => Math.Clamp(serverConfidence, 50, 88)
        };

        if (aiConfidence <= 10 || aiConfidence > 100)
            return guardedServer;

        var guardedAi = verdict switch
        {
            "buy" => Math.Clamp(aiConfidence, 70, 95),
            "skip" => Math.Clamp(aiConfidence, 35, 62),
            _ => Math.Clamp(aiConfidence, 50, 88)
        };

        return (int)Math.Round(guardedAi * 0.7 + guardedServer * 0.3);
    }

    private static int BuildAdvisorFitPercent(double score, string verdict, Product product, ProductAdvisorRequest request, bool hasBetterMatch)
    {
        var fit = 45 + score * 1.1;
        var hasNeed = !string.IsNullOrWhiteSpace(request.NeedDescription)
            || !string.IsNullOrWhiteSpace(request.RoomType)
            || request.Budget.GetValueOrDefault() > 0;
        var budget = request.Budget.GetValueOrDefault();
        var overBudget = budget > 0 && product.UnitPrice > budget;
        var stock = GetProductStock(product);

        if (!hasNeed)
            fit -= 5;
        if (hasBetterMatch)
            fit -= 8;
        if (!product.Reviews.Any(r => r.IsApproved))
            fit -= 3;
        if (stock <= 0)
            fit = Math.Min(fit, 48);
        if (overBudget)
            fit = Math.Min(fit, 55);

        var rounded = (int)Math.Round(fit);
        return verdict switch
        {
            "buy" => Math.Clamp(rounded, 76, 95),
            "skip" => Math.Clamp(rounded, 45, 62),
            _ => Math.Clamp(rounded, 55, 84)
        };
    }

    private static IReadOnlyList<string> GetJsonStringArray(JsonElement root, string propertyName, IReadOnlyList<string> fallback)
        => root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Cast<string>()
                .Take(5)
                .ToList()
            : fallback;

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSingleToCart([FromBody] AddSingleToCartRequest request)
    {
        try
        {
            var userId = await _userService.GetCurrentUserIdAsync();
            if (userId == null)
                return Json(new { ok = false, message = _localizer.Get("AiStylist:CartLoginRequired") });

            var product = await _context.Products
                .Where(p => p.Id == request.ProductId && !p.Discontinued)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Images)
                .FirstOrDefaultAsync();

            if (product == null)
                return Json(new { ok = false, message = _localizer.Get("AiStylist:CartProductNotFound") });

            var success = await AddProductToCartAsync(userId.Value, product, 1);
            if (success)
            {
                return Json(new { ok = true, message = _localizer.Get("AiStylist:CartAddSuccess", product.ProductName) });
            }
            else
            {
                return Json(new { ok = false, message = _localizer.Get("AiStylist:CartOutOfStock") });
            }
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, message = _localizer.Get("AiStylist:CartAddError", ex.Message) });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TrySingleInRoom3D([FromBody] TrySingleInRoom3DRequest request, CancellationToken cancellationToken)
    {
        if (request.ProductId <= 0)
            return Json(new { ok = false, message = _localizer.Get("AiStylist:InvalidProduct") });

        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && !p.Discontinued, cancellationToken);

        if (product == null)
            return Json(new { ok = false, message = _localizer.Get("AiStylist:CartProductNotFound") });

        if (!Room3DProductCatalog.TryGetRoomProductId(product, out _))
            return Json(new { ok = false, message = "Sản phẩm này chưa có model 3D để thử trong phòng." });

        var distinctIds = new List<int> { request.ProductId };
        HttpContext.Session.SetString(AiRoom3DSelectionSessionKey, JsonSerializer.Serialize(distinctIds));
        return Json(new { ok = true, redirectUrl = Url.Action("Index", "Room3D", new { source = "ai" }) });
    }

    public class TrySingleInRoom3DRequest
    {
        public int ProductId { get; set; }
    }

    private async Task<AiStylistPageViewModel?> BuildProductSeedViewModelAsync(int productId, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Where(p => p.Id == productId && !p.Discontinued)
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(cancellationToken);

        if (product == null)
            return null;

        var categoryName = product.Category?.CategoryName ?? "sản phẩm nội thất";
        var stock = product.Variants.Any(v => v.IsActive)
            ? product.Variants.Where(v => v.IsActive).Sum(v => Math.Max(v.StockQuantity, 0))
            : 100;
        var request = new StyleSurveyRequest
        {
            RoomType = GuessRoomType(categoryName, product.ProductName),
            Priority = "cozy",
            Budget = product.UnitPrice > 0 ? product.UnitPrice * 4 : null,
            UserDescription = BuildProductSeedPrompt(product, categoryName)
        };

        var (result, candidates) = await _recommendationService.RecommendFromStyleSurveyAsync(request, cancellationToken);
        var products = await _validator.BuildValidatedProductsAsync(result, candidates, cancellationToken);
        SaveLastStylistState(request, result, candidates.Count);

        var chatHistory = new List<AiStylistChatMessage>
        {
            new()
            {
                Role = "user",
                Mode = _localizer.Get("AiStylist:ModeTextDesc"),
                Text = $"Hãy phối không gian xoay quanh {product.ProductName}.",
                Timestamp = DateTime.UtcNow
            },
            new()
            {
                Role = "assistant",
                Mode = _localizer.Get("AiStylist:ModeTextDesc"),
                Text = BuildProductSeedReply(product, categoryName, stock, Room3DProductCatalog.TryGetRoomProductId(product, out _)),
                Timestamp = DateTime.UtcNow
            }
        };

        HttpContext.Session.SetString(AiStylistChatHistorySessionKey, JsonSerializer.Serialize(chatHistory));

        return new AiStylistPageViewModel
        {
            Request = request,
            Result = result,
            Products = products,
            IsAiConfigured = _recommendationService.IsAiConfigured,
            CandidateCount = candidates.Count,
            ChatHistory = chatHistory,
            ErrorMessage = products.Any() ? null : _localizer.Get("AiStylist:ErrNoMatchingProducts")
        };
    }

    private static string BuildProductSeedPrompt(Product product, string categoryName)
    {
        var description = string.IsNullOrWhiteSpace(product.QuantityPerUnit)
            ? "Chưa có mô tả chi tiết."
            : product.QuantityPerUnit;

        return $"Tôi đang xem sản phẩm {product.ProductName}, nhóm {categoryName}, giá {product.UnitPrice:N0} VND, mô tả: {description}. Hãy đề xuất cách phối và các món đi kèm để sản phẩm này trở thành điểm nhấn chính.";
    }

    private static string BuildProductSeedReply(Product product, string categoryName, int stock, bool hasRoom3DModel)
    {
        var model3DNote = hasRoom3DModel
            ? "Sản phẩm này có thể thử trực tiếp trong phòng 3D."
            : "Sản phẩm này chưa có model 3D, nên mình sẽ ưu tiên gợi ý phối bằng sản phẩm catalog phù hợp.";

        return $"Mình sẽ lấy {product.ProductName} làm điểm neo cho bản phối. Đây là nhóm {categoryName}, giá khoảng {product.UnitPrice:N0} VND, tồn kho hiện ghi nhận {stock:N0}. {model3DNote} Bên dưới là một bản phối gợi ý để bạn xem nhanh, rồi mình có thể tinh chỉnh tiếp theo phòng, màu, ngân sách hoặc mood bạn muốn.";
    }

    private static string GuessRoomType(string categoryName, string productName)
    {
        var text = string.Join(" ", categoryName, productName).ToLowerInvariant();
        if (ContainsAny(text, "bed", "giường", "mattress", "nệm", "wardrobe", "tủ áo"))
            return "bedroom";
        if (ContainsAny(text, "desk", "office", "work", "làm việc"))
            return "office";
        if (ContainsAny(text, "dining", "ăn", "bàn ăn"))
            return "dining_room";

        return "living_room";
    }

    private static bool ContainsAny(string value, params string[] keywords)
        => keywords.Any(value.Contains);

    private void AppendChatTurnForText(string userMsg, string assistantMsg)
    {
        var history = LoadChatHistory().ToList();
        history.Add(new AiStylistChatMessage
        {
            Role = "user",
            Mode = _localizer.Get("AiStylist:ModeTextDesc"),
            Text = userMsg,
            Timestamp = DateTime.UtcNow
        });
        history.Add(new AiStylistChatMessage
        {
            Role = "assistant",
            Mode = _localizer.Get("AiStylist:ModeTextDesc"),
            Text = assistantMsg,
            Timestamp = DateTime.UtcNow
        });

        var trimmed = history.TakeLast(16).ToList();
        HttpContext.Session.SetString(AiStylistChatHistorySessionKey, JsonSerializer.Serialize(trimmed));
    }

    private static string ExtractJson(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
            return trimmed;

        var match = System.Text.RegularExpressions.Regex.Match(trimmed, "\\{[\\s\\S]*\\}");
        return match.Success ? match.Value : "{}";
    }

    public class AddSingleToCartRequest
    {
        public int ProductId { get; set; }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComboToCart([FromForm] List<int> productIds, string? returnUrl = null)
    {
        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == null)
        {
            var loginReturnUrl = Url.Action(nameof(Stylist), "Ai");
            return RedirectToAction("Login", "Account", new { returnUrl = loginReturnUrl });
        }

        var distinctIds = productIds.Distinct().Take(12).ToList();
        if (!distinctIds.Any())
        {
            TempData["ToastWarning"] = _localizer.Get("AiStylist:CartComboEmpty");
            return RedirectToLocal(returnUrl);
        }

        var products = await LoadCartProductsAsync(distinctIds);
        var addedCount = 0;

        foreach (var product in products)
        {
            if (await AddProductToCartAsync(userId.Value, product, 1))
                addedCount++;
        }

        TempData[addedCount > 0 ? "ToastSuccess" : "ToastWarning"] = addedCount > 0
            ? _localizer.Get("AiStylist:CartComboAddSuccess", addedCount)
            : _localizer.Get("AiStylist:CartComboOutOfStock");

        return RedirectToLocal(returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult TryInRoom3D([FromForm] List<int> productIds)
    {
        var distinctIds = productIds.Distinct().Take(12).ToList();
        if (!distinctIds.Any())
        {
            TempData["ToastWarning"] = _localizer.Get("AiStylist:TryComboEmpty");
            return RedirectToAction(nameof(Stylist));
        }

        HttpContext.Session.SetString(AiRoom3DSelectionSessionKey, JsonSerializer.Serialize(distinctIds));
        return RedirectToAction("Index", "Room3D", new { source = "ai" });
    }

    [HttpGet]
    public async Task<IActionResult> Room3DSelection(CancellationToken cancellationToken)
    {
        var raw = HttpContext.Session.GetString(AiRoom3DSelectionSessionKey);
        if (string.IsNullOrWhiteSpace(raw))
            return Json(new { products = Array.Empty<object>() });

        var ids = JsonSerializer.Deserialize<List<int>>(raw) ?? new List<int>();
        if (!ids.Any())
            return Json(new { products = Array.Empty<object>() });

        var products = await _context.Products
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id) && !p.Discontinued)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .ToListAsync(cancellationToken);

        var response = ids
            .Select(id => products.FirstOrDefault(p => p.Id == id))
            .Where(p => p != null)
            .Where(p => Room3DProductCatalog.TryGetRoomProductId(p!, out _))
            .Select(p => new
            {
                roomProductId = Room3DProductCatalog.TryGetRoomProductId(p!, out var roomProductId) ? roomProductId : null,
                id = $"catalog-{p!.Id}",
                catalogProductId = p.Id,
                name = p.ProductName,
                category = p.Category?.CategoryName ?? "Decor",
                price = p.UnitPrice,
                thumbnail = p.Images.OrderByDescending(i => i.IsMain).ThenBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl
            })
            .ToList();

        return Json(new { products = response });
    }

    private async Task<List<Product>> LoadCartProductsAsync(IReadOnlyList<int> productIds)
    {
        return await _context.Products
            .Where(p => productIds.Contains(p.Id) && !p.Discontinued)
            .Include(p => p.Images)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Images)
            .ToListAsync();
    }

    private async Task<bool> AddProductToCartAsync(int userId, Product product, int quantity)
    {
        var activeVariants = product.Variants.Where(v => v.IsActive).ToList();
        var variant = activeVariants.FirstOrDefault(v => v.StockQuantity >= quantity)
            ?? activeVariants.FirstOrDefault();

        if (activeVariants.Any() && variant == null)
            return false;

        if (variant != null && variant.StockQuantity <= 0)
            return false;

        var variantId = variant?.Id ?? 0;
        var priceAdjustment = variant?.PriceAdjustment ?? 0;
        var finalPrice = product.UnitPrice + priceAdjustment;
        var variantName = variant == null
            ? null
            : string.Join(" / ", new[] { variant.Color, variant.Size }.Where(v => !string.IsNullOrWhiteSpace(v)));
        var image = variant?.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl
            ?? product.Images.OrderByDescending(i => i.IsMain).ThenBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl;
        var priceBreakdown = priceAdjustment != 0
            ? $"Base: {product.UnitPrice:N0} + Variant: +{priceAdjustment:N0} = {finalPrice:N0} VND"
            : "";

        await _cartService.AddToCartAsync(
            userId,
            product.Id,
            variantId,
            finalPrice,
            product.UnitPrice,
            priceAdjustment,
            priceBreakdown,
            product.ProductName,
            variantName,
            image,
            quantity);

        return true;
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(RedirectUrlSanitizer.EscapeHeaderValue(returnUrl));
        return RedirectToAction(nameof(Stylist));
    }

    private void SaveLastStylistState(StyleSurveyRequest request, AiRecommendationResult result, int candidateCount)
    {
        var state = new AiStylistSessionState
        {
            Request = request,
            Result = result,
            CandidateCount = candidateCount
        };
        HttpContext.Session.SetString(AiStylistLastResultSessionKey, JsonSerializer.Serialize(state));
    }

    private AiStylistSessionState? LoadLastStylistState()
    {
        var raw = HttpContext.Session.GetString(AiStylistLastResultSessionKey);
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        try
        {
            return JsonSerializer.Deserialize<AiStylistSessionState>(raw);
        }
        catch (JsonException)
        {
            HttpContext.Session.Remove(AiStylistLastResultSessionKey);
            return null;
        }
    }

    private IReadOnlyList<AiStylistChatMessage> AppendChatTurn(
        StyleSurveyRequest request,
        AiRecommendationResult result,
        int productCount)
    {
        var history = LoadChatHistory().ToList();
        history.Add(new AiStylistChatMessage
        {
            Role = "user",
            Mode = _localizer.Get("AiStylist:ModeTextDesc"),
            Text = BuildUserMessage(request),
            Timestamp = DateTime.UtcNow
        });
        history.Add(new AiStylistChatMessage
        {
            Role = "assistant",
            Mode = _localizer.Get("AiStylist:ModeTextDesc"),
            Text = productCount > 0
                ? _localizer.Get("AiStylist:HistoryRecommendSuccess", result.ConceptName, productCount)
                : _localizer.Get("AiStylist:HistoryRecommendFallback"),
            Timestamp = DateTime.UtcNow
        });

        var trimmed = history.TakeLast(16).ToList();
        HttpContext.Session.SetString(AiStylistChatHistorySessionKey, JsonSerializer.Serialize(trimmed));
        return trimmed;
    }

    private IReadOnlyList<AiStylistChatMessage> LoadChatHistory()
    {
        var raw = HttpContext.Session.GetString(AiStylistChatHistorySessionKey);
        if (string.IsNullOrWhiteSpace(raw))
            return Array.Empty<AiStylistChatMessage>();

        try
        {
            return JsonSerializer.Deserialize<List<AiStylistChatMessage>>(raw) ?? new List<AiStylistChatMessage>();
        }
        catch (JsonException)
        {
            HttpContext.Session.Remove(AiStylistChatHistorySessionKey);
            return Array.Empty<AiStylistChatMessage>();
        }
    }

    private string BuildUserMessage(StyleSurveyRequest request)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(request.UserDescription))
            parts.Add(request.UserDescription.Trim());
        if (!string.IsNullOrWhiteSpace(request.Area))
            parts.Add(_localizer.Get("AiStylist:HistoryRoomArea", request.Area.Trim()));
        var budget = request.Budget.GetValueOrDefault();
        if (budget > 0)
            parts.Add(_localizer.Get("AiStylist:HistoryBudget", budget));
        if (!string.IsNullOrWhiteSpace(request.PreferredColors))
            parts.Add(_localizer.Get("AiStylist:HistoryLike", request.PreferredColors.Trim()));
        if (!string.IsNullOrWhiteSpace(request.AvoidColors))
            parts.Add(_localizer.Get("AiStylist:HistoryAvoid", request.AvoidColors.Trim()));
 
        return parts.Any()
            ? string.Join(" · ", parts)
            : _localizer.Get("AiStylist:HistoryDefaultRequest");
    }

    private async Task<IReadOnlyList<AiRecommendedProductViewModel>> RebuildProductsFromSavedResultAsync(
        AiRecommendationResult result,
        CancellationToken cancellationToken)
    {
        var savedCandidates = result.RecommendedProducts
            .Select(item => new AiProductCandidate { ProductId = item.ProductId })
            .ToList();

        return await _validator.BuildValidatedProductsAsync(result, savedCandidates, cancellationToken);
    }

    private sealed class AiStylistSessionState
    {
        public StyleSurveyRequest? Request { get; set; }
        public AiRecommendationResult? Result { get; set; }
        public int CandidateCount { get; set; }
    }

    private sealed record AdvisorScoredProduct(Product Product, double Score, string Reason);
}
