using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WebActionResults.Models;
using WebActionResults.ViewModels.Ai;

namespace WebActionResults.Services.Ai;

public interface IAiProductCandidateService
{
    Task<IReadOnlyList<AiProductCandidate>> GetCandidatesAsync(AiCandidateFilter filter, CancellationToken cancellationToken = default);
    void Invalidate();
}

public class AiProductCandidateService : IAiProductCandidateService
{
    private const string CacheKey = "ai:product-candidates:base";
    private readonly ShopDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly AiOptions _options;

    public AiProductCandidateService(ShopDbContext context, IMemoryCache cache, AiOptions options)
    {
        _context = context;
        _cache = cache;
        _options = options;
    }

    public async Task<IReadOnlyList<AiProductCandidate>> GetCandidatesAsync(AiCandidateFilter filter, CancellationToken cancellationToken = default)
    {
        var baseCandidates = await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CandidateCacheMinutes);
            entry.SetPriority(CacheItemPriority.Normal);
            return await BuildBaseCandidatesAsync(cancellationToken);
        }) ?? new List<AiProductCandidate>();

        return RankAndTrim(baseCandidates, filter);
    }

    public void Invalidate()
    {
        _cache.Remove(CacheKey);
    }

    private async Task<List<AiProductCandidate>> BuildBaseCandidatesAsync(CancellationToken cancellationToken)
    {
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => !p.Discontinued)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .OrderBy(p => p.ProductName)
            .ToListAsync(cancellationToken);

        return products
            .Select(product =>
            {
                var activeVariants = product.Variants.Where(v => v.IsActive).ToList();
                var stock = activeVariants.Any()
                    ? activeVariants.Sum(v => Math.Max(v.StockQuantity, 0))
                    : 100;
                var colors = activeVariants
                    .Select(v => v.Color)
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Cast<string>()
                    .ToList();
                var sizes = activeVariants
                    .Select(v => v.Size)
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Cast<string>()
                    .ToList();
                var image = product.Images
                    .OrderByDescending(i => i.IsMain)
                    .ThenBy(i => i.DisplayOrder)
                    .FirstOrDefault()
                    ?.ImageUrl;

                return new AiProductCandidate
                {
                    ProductId = product.Id,
                    Name = product.ProductName,
                    Category = product.Category?.CategoryName ?? "Other",
                    Price = product.UnitPrice,
                    StockQuantity = stock,
                    Description = product.QuantityPerUnit,
                    ImageUrl = image,
                    Colors = colors,
                    Sizes = sizes,
                    Tags = BuildTags(product.Category?.CategoryName, product.ProductName, product.QuantityPerUnit, colors),
                    ScoreHint = 0
                };
            })
            .Where(candidate => candidate.StockQuantity > 0)
            .ToList();
    }

    private List<AiProductCandidate> RankAndTrim(IReadOnlyList<AiProductCandidate> candidates, AiCandidateFilter filter)
    {
        var max = Math.Clamp(filter.MaxCandidates <= 0 ? _options.MaxCatalogItems : filter.MaxCandidates, 20, 300);
        var budget = filter.Budget.GetValueOrDefault();
        var roomTokens = Tokens(filter.RoomType);
        var styleTokens = Tokens(filter.Style);
        var descriptionTokens = Tokens(filter.UserDescription);
        var priorityTokens = Tokens(filter.Priority);
        var preferredColors = filter.PreferredColors.SelectMany(Tokens).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var avoidColors = filter.AvoidColors.SelectMany(Tokens).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var ranked = candidates
            .Select(candidate =>
            {
                var score = 0d;
                var searchable = string.Join(" ", candidate.Name, candidate.Category, candidate.Description, string.Join(" ", candidate.Tags), string.Join(" ", candidate.Colors)).ToLowerInvariant();

                score += candidate.ImageUrl != null ? 8 : 0;
                score += Math.Min(candidate.StockQuantity, 20) * 0.4;

                if (budget > 0)
                {
                    score += candidate.Price <= budget ? 18 : -Math.Min(25, (double)((candidate.Price - budget) / Math.Max(budget, 1) * 20));
                }

                score += roomTokens.Count(token => searchable.Contains(token)) * 10;
                score += styleTokens.Count(token => searchable.Contains(token)) * 7;
                score += descriptionTokens.Count(token => searchable.Contains(token)) * 4;
                score += priorityTokens.Count(token => searchable.Contains(token)) * 6;
                score += preferredColors.Count(token => searchable.Contains(token)) * 5;
                score -= avoidColors.Count(token => searchable.Contains(token)) * 12;

                return WithScore(candidate, score);
            })
            .OrderByDescending(c => c.ScoreHint)
            .ThenBy(c => c.Price)
            .ToList();

        if (ranked.Count <= max)
            return ranked;

        var selected = new List<AiProductCandidate>();
        foreach (var group in ranked.GroupBy(c => NormalizeCategory(c.Category)).OrderByDescending(g => g.Max(c => c.ScoreHint)))
        {
            selected.AddRange(group.Take(Math.Max(3, max / 12)));
            if (selected.Count >= max)
                break;
        }

        if (selected.Count < max)
        {
            selected.AddRange(ranked.Where(c => selected.All(s => s.ProductId != c.ProductId)).Take(max - selected.Count));
        }

        return selected
            .DistinctBy(c => c.ProductId)
            .OrderByDescending(c => c.ScoreHint)
            .Take(max)
            .ToList();

        AiProductCandidate WithScore(AiProductCandidate candidate, double score)
        {
            candidate.ScoreHint = score;
            return candidate;
        }
    }

    private static IReadOnlyList<string> BuildTags(string? category, string name, string? description, IReadOnlyList<string> colors)
    {
        var text = string.Join(" ", category, name, description, string.Join(" ", colors)).ToLowerInvariant();
        var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var token in Tokens(text))
            tags.Add(token);

        if (text.Contains("sofa") || text.Contains("couch")) tags.Add("living_room");
        if (text.Contains("bed")) tags.Add("bedroom");
        if (text.Contains("desk") || text.Contains("office")) tags.Add("office");
        if (text.Contains("lamp") || text.Contains("light")) tags.Add("lighting");
        if (text.Contains("plant") || text.Contains("tree")) tags.Add("green");
        if (text.Contains("wood") || text.Contains("gỗ")) tags.Add("warm");
        if (text.Contains("white") || text.Contains("beige") || text.Contains("cream")) tags.Add("bright");

        return tags.Take(20).ToList();
    }

    private static string NormalizeCategory(string value)
        => value.Trim().ToLowerInvariant();

    private static IReadOnlyList<string> Tokens(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Array.Empty<string>();

        return value
            .ToLowerInvariant()
            .Replace("_", " ")
            .Replace("-", " ")
            .Split(new[] { ' ', ',', ';', '/', '|', '.', ':' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(token => token.Length >= 2)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
