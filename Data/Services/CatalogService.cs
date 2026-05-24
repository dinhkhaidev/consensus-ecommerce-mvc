using Microsoft.EntityFrameworkCore;
using WebActionResults.Data.Entities;
using WebActionResults.Data.Repositories;
using WebActionResults.Models;

namespace WebActionResults.Data.Services;

public interface ICatalogService
{
    // Categories
    Task<List<Category>> GetAllCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(int id);
    Task<List<Category>> GetCategoriesWithProductsAsync();

    // Products
    Task<List<Product>> GetAllProductsAsync();
    Task<Product?> GetProductByIdAsync(int id);
    Task<Product?> GetProductWithDetailsAsync(int id);
    Task<List<Product>> GetProductsByCategoryAsync(int categoryId);
    Task<List<Product>> GetRecommendedProductsAsync(Product product, int count = 8);
    Task<List<Product>> GetFeaturedProductsAsync(int count = 8);
    Task<List<Product>> SearchProductsAsync(string keyword);
    Task<(List<Product> Items, int TotalCount)> GetProductsPaginatedAsync(int page, int pageSize, string? sort = null);
    Task<(List<Product> Items, int TotalCount)> GetProductsByCategoryPaginatedAsync(int categoryId, int page, int pageSize, string? sort = null);
    Task<(List<Product> Items, int TotalCount)> SearchProductsPaginatedAsync(string keyword, int page, int pageSize, string? sort = null);
    Task<(List<Product> Items, int TotalCount)> GetProductsByPriceRangeAsync(int? minPrice, int? maxPrice, int page, int pageSize, string? sort = null);
    Task<(List<Product> Items, int TotalCount)> GetProductsByCategoryAndPriceRangeAsync(int categoryId, int? minPrice, int? maxPrice, int page, int pageSize, string? sort = null);

    // Variants & Images
    Task<List<ProductVariant>> GetProductVariantsAsync(int productId);
    Task<List<ProductImage>> GetProductImagesAsync(int productId);

    // Reviews
    Task<List<Review>> GetProductReviewsAsync(int productId);
    Task AddReviewAsync(Review review);
}

public class CatalogService : ICatalogService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly ShopDbContext _context;

    public CatalogService(ICategoryRepository categoryRepository, IProductRepository productRepository, ShopDbContext context)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _context = context;
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
        => await _categoryRepository.GetAllAsync();

    public async Task<Category?> GetCategoryByIdAsync(int id)
        => await _categoryRepository.GetByIdAsync(id);

    public async Task<List<Category>> GetCategoriesWithProductsAsync()
        => await _categoryRepository.GetWithProductsAsync();

    public async Task<List<Product>> GetAllProductsAsync()
        => await _productRepository.GetAllAsync();

    public async Task<Product?> GetProductByIdAsync(int id)
        => await _productRepository.GetByIdAsync(id);

    public async Task<Product?> GetProductWithDetailsAsync(int id)
        => await _productRepository.GetByIdWithDetailsAsync(id);

    public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId)
        => await _productRepository.GetByCategoryAsync(categoryId);

    public async Task<List<Product>> GetRecommendedProductsAsync(Product product, int count = 8)
    {
        var orderIdsWithProduct = await _context.OrderItems
            .Where(i => i.ProductId == product.Id)
            .Select(i => i.OrderId)
            .Distinct()
            .ToListAsync();

        var coPurchaseScores = orderIdsWithProduct.Any()
            ? await _context.OrderItems
                .Where(i => orderIdsWithProduct.Contains(i.OrderId) && i.ProductId != product.Id)
                .GroupBy(i => i.ProductId)
                .Select(g => new { ProductId = g.Key, Score = g.Sum(i => i.Quantity) })
                .ToDictionaryAsync(i => i.ProductId, i => i.Score)
            : new Dictionary<int, int>();

        var candidates = await _context.Products
            .Where(p => !p.Discontinued && p.Id != product.Id)
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.Reviews)
            .ToListAsync();

        var currentCategory = product.Category?.CategoryName ?? string.Empty;
        var currentTokens = GetRecommendationTokens(product.ProductName);

        return candidates
            .Select(candidate => new
            {
                Product = candidate,
                Score =
                    GetCoPurchaseScore(candidate.Id, coPurchaseScores) +
                    GetCategoryScore(product, candidate, currentCategory) +
                    GetSupplierScore(product, candidate) +
                    GetPriceScore(product.UnitPrice, candidate.UnitPrice) +
                    GetMerchandisingScore(candidate) +
                    GetNameAffinityScore(currentTokens, candidate.ProductName)
            })
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Product.Reviews.Where(r => r.IsApproved).DefaultIfEmpty().Average(r => r?.Rating ?? 0))
            .ThenBy(x => x.Product.ProductName)
            .Take(count)
            .Select(x => x.Product)
            .ToList();
    }

    public async Task<List<Product>> GetFeaturedProductsAsync(int count = 8)
        => await _productRepository.GetFeaturedAsync(count);

    public async Task<List<Product>> SearchProductsAsync(string keyword)
        => await _productRepository.SearchAsync(keyword);

    public async Task<List<ProductVariant>> GetProductVariantsAsync(int productId)
        => await _productRepository.GetVariantsAsync(productId);

    public async Task<List<ProductImage>> GetProductImagesAsync(int productId)
        => await _productRepository.GetImagesAsync(productId);

    public async Task<List<Review>> GetProductReviewsAsync(int productId)
        => await _productRepository.GetReviewsAsync(productId);

    public async Task AddReviewAsync(Review review)
    {
        review.CreatedAt = DateTime.UtcNow;
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
    }

    public async Task<(List<Product> Items, int TotalCount)> GetProductsPaginatedAsync(int page, int pageSize, string? sort = null)
        => await _productRepository.GetAllPaginatedAsync(page, pageSize, sort);

    public async Task<(List<Product> Items, int TotalCount)> GetProductsByCategoryPaginatedAsync(int categoryId, int page, int pageSize, string? sort = null)
        => await _productRepository.GetByCategoryPaginatedAsync(categoryId, page, pageSize, sort);

    public async Task<(List<Product> Items, int TotalCount)> SearchProductsPaginatedAsync(string keyword, int page, int pageSize, string? sort = null)
        => await _productRepository.SearchPaginatedAsync(keyword, page, pageSize, sort);

    public async Task<(List<Product> Items, int TotalCount)> GetProductsByPriceRangeAsync(int? minPrice, int? maxPrice, int page, int pageSize, string? sort = null)
        => await _productRepository.GetByPriceRangeAsync(minPrice, maxPrice, page, pageSize, sort);

    public async Task<(List<Product> Items, int TotalCount)> GetProductsByCategoryAndPriceRangeAsync(int categoryId, int? minPrice, int? maxPrice, int page, int pageSize, string? sort = null)
        => await _productRepository.GetByCategoryAndPriceRangeAsync(categoryId, minPrice, maxPrice, page, pageSize, sort);

    private static double GetCoPurchaseScore(int productId, IReadOnlyDictionary<int, int> coPurchaseScores)
        => coPurchaseScores.TryGetValue(productId, out var score) ? Math.Min(score, 6) * 12 : 0;

    private static double GetCategoryScore(Product current, Product candidate, string currentCategory)
    {
        var score = current.CategoryID == candidate.CategoryID ? 6 : 0;
        if (IsComplementaryCategory(currentCategory, candidate.Category?.CategoryName ?? string.Empty))
        {
            score += 9;
        }

        return score;
    }

    private static double GetSupplierScore(Product current, Product candidate)
        => current.SupplierID.HasValue && current.SupplierID == candidate.SupplierID ? 3 : 0;

    private static double GetPriceScore(decimal currentPrice, decimal candidatePrice)
    {
        if (currentPrice <= 0 || candidatePrice <= 0)
        {
            return 0;
        }

        var higher = Math.Max(currentPrice, candidatePrice);
        var lower = Math.Min(currentPrice, candidatePrice);
        var closeness = (double)(lower / higher);
        return closeness * 5;
    }

    private static double GetMerchandisingScore(Product candidate)
    {
        var approvedReviews = candidate.Reviews.Where(r => r.IsApproved).ToList();
        var averageRating = approvedReviews.Any() ? approvedReviews.Average(r => r.Rating) : 0;
        var reviewScore = averageRating * 0.7 + Math.Min(approvedReviews.Count, 20) * 0.1;
        var hasStock = candidate.Variants.Any(v => v.IsActive && v.StockQuantity > 0);
        var hasImage = candidate.Images.Any(i => !string.IsNullOrWhiteSpace(i.ImageUrl));

        return reviewScore + (hasStock ? 2 : -4) + (hasImage ? 1.5 : 0);
    }

    private static double GetNameAffinityScore(HashSet<string> currentTokens, string candidateName)
    {
        if (!currentTokens.Any())
        {
            return 0;
        }

        var candidateTokens = GetRecommendationTokens(candidateName);
        return Math.Min(currentTokens.Intersect(candidateTokens).Count(), 3) * 0.8;
    }

    private static HashSet<string> GetRecommendationTokens(string value)
        => value
            .ToLowerInvariant()
            .Split(new[] { ' ', '-', '_', '/', ',', '.', '(', ')' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(token => token.Length >= 3)
            .ToHashSet();

    private static bool IsComplementaryCategory(string currentCategory, string candidateCategory)
    {
        var current = currentCategory.ToLowerInvariant();
        var candidate = candidateCategory.ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(current) || string.IsNullOrWhiteSpace(candidate) || current == candidate)
        {
            return false;
        }

        var roomCore = ContainsAny(current, "sofa", "chair", "ghế", "seat", "bàn", "table");
        var roomAccent = ContainsAny(candidate, "lamp", "đèn", "rug", "thảm", "decor", "trang trí", "kệ", "shelf", "gối", "pillow");
        var bedroomCore = ContainsAny(current, "bed", "giường", "mattress", "nệm", "wardrobe", "tủ");
        var bedroomAccent = ContainsAny(candidate, "nightstand", "tab", "đèn", "lamp", "gối", "pillow", "chăn", "blanket");
        var diningCore = ContainsAny(current, "dining", "ăn", "table", "bàn");
        var diningAccent = ContainsAny(candidate, "chair", "ghế", "bench", "băng", "decor", "trang trí", "đèn", "lamp");

        return roomCore && roomAccent || bedroomCore && bedroomAccent || diningCore && diningAccent;
    }

    private static bool ContainsAny(string value, params string[] keywords)
        => keywords.Any(value.Contains);
}
