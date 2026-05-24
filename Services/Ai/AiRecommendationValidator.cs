using Microsoft.EntityFrameworkCore;
using WebActionResults.Models;
using WebActionResults.Utilities;
using WebActionResults.ViewModels.Ai;

namespace WebActionResults.Services.Ai;

public interface IAiRecommendationValidator
{
    Task<IReadOnlyList<AiRecommendedProductViewModel>> BuildValidatedProductsAsync(
        AiRecommendationResult result,
        IReadOnlyList<AiProductCandidate> candidates,
        CancellationToken cancellationToken = default);
}

public class AiRecommendationValidator : IAiRecommendationValidator
{
    private readonly ShopDbContext _context;

    public AiRecommendationValidator(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AiRecommendedProductViewModel>> BuildValidatedProductsAsync(
        AiRecommendationResult result,
        IReadOnlyList<AiProductCandidate> candidates,
        CancellationToken cancellationToken = default)
    {
        var allowedIds = candidates.Select(c => c.ProductId).ToHashSet();
        var validAiItems = result.RecommendedProducts
            .Where(item => allowedIds.Contains(item.ProductId))
            .GroupBy(item => item.ProductId)
            .Select(group => group.OrderBy(item => item.Rank).First())
            .Take(12)
            .ToList();

        if (!validAiItems.Any())
            return Array.Empty<AiRecommendedProductViewModel>();

        var ids = validAiItems.Select(item => item.ProductId).ToList();
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id) && !p.Discontinued)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .ToListAsync(cancellationToken);

        return validAiItems
            .Select((item, index) =>
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product == null)
                    return null;

                var activeVariants = product.Variants.Where(v => v.IsActive).ToList();
                var stock = activeVariants.Any() ? activeVariants.Sum(v => Math.Max(v.StockQuantity, 0)) : 100;
                if (stock <= 0)
                    return null;

                return new AiRecommendedProductViewModel
                {
                    ProductId = product.Id,
                    ProductName = product.ProductName,
                    CategoryName = product.Category?.CategoryName ?? "Other",
                    UnitPrice = product.UnitPrice,
                    StockQuantity = stock,
                    ImageUrl = product.Images
                        .OrderByDescending(i => i.IsMain)
                        .ThenBy(i => i.DisplayOrder)
                        .FirstOrDefault()
                        ?.ImageUrl,
                    Reason = item.Reason,
                    Rank = item.Rank > 0 ? item.Rank : index + 1,
                    HasRoom3DModel = Room3DProductCatalog.TryGetRoomProductId(product, out _)
                };
            })
            .Where(item => item != null)
            .Cast<AiRecommendedProductViewModel>()
            .OrderBy(item => item.Rank)
            .ToList();
    }
}
