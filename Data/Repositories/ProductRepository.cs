using Microsoft.EntityFrameworkCore;
using WebActionResults.Data.Entities;
using WebActionResults.Models;

namespace WebActionResults.Data.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetByIdWithDetailsAsync(int id);
    Task<List<Product>> GetByCategoryAsync(int categoryId);
    Task<List<Product>> GetFeaturedAsync(int count = 8);
    Task<List<Product>> SearchAsync(string keyword);
    Task<List<ProductVariant>> GetVariantsAsync(int productId);
    Task<List<ProductImage>> GetImagesAsync(int productId);
    Task<List<Review>> GetReviewsAsync(int productId, bool approvedOnly = true);
    Task<bool> DeductStockAsync(int variantId, int quantity);
    Task<bool> RestoreStockAsync(int variantId, int quantity);

    // Paginated methods
    Task<(List<Product> Items, int TotalCount)> GetAllPaginatedAsync(int page, int pageSize, string? sort = null, int? minRating = null);
    Task<(List<Product> Items, int TotalCount)> GetByCategoryPaginatedAsync(int categoryId, int page, int pageSize, string? sort = null, int? minRating = null);
    Task<(List<Product> Items, int TotalCount)> SearchPaginatedAsync(string keyword, int page, int pageSize, string? sort = null, int? minRating = null);
    Task<(List<Product> Items, int TotalCount)> GetByPriceRangeAsync(int? minPrice, int? maxPrice, int page, int pageSize, string? sort = null, int? minRating = null);
    Task<(List<Product> Items, int TotalCount)> GetByCategoryAndPriceRangeAsync(int categoryId, int? minPrice, int? maxPrice, int page, int pageSize, string? sort = null, int? minRating = null);
}

public class ProductRepository : IProductRepository
{
    private readonly ShopDbContext _context;

    public ProductRepository(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAllAsync()
        => await _context.Products
            .Where(p => !p.Discontinued)
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .OrderBy(p => p.ProductName)
            .ToListAsync();

    public async Task<Product?> GetByIdAsync(int id)
        => await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Product?> GetByIdWithDetailsAsync(int id)
        => await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Images)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<List<Product>> GetByCategoryAsync(int categoryId)
        => await _context.Products
            .Where(p => p.Category.Id == categoryId && !p.Discontinued)
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .OrderBy(p => p.ProductName)
            .ToListAsync();

    public async Task<List<Product>> GetFeaturedAsync(int count = 8)
        => await _context.Products
            .Where(p => !p.Discontinued)
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .OrderByDescending(p => p.Id)
            .Take(count)
            .ToListAsync();

    public async Task<List<Product>> SearchAsync(string keyword)
    {
        var lowerKeyword = keyword.ToLower();
        return await _context.Products
            .Where(p => !p.Discontinued &&
                (p.ProductName.ToLower().Contains(lowerKeyword) ||
                 (p.Category != null && p.Category.CategoryName.ToLower().Contains(lowerKeyword))))
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .OrderBy(p => p.ProductName)
            .ToListAsync();
    }

    public async Task<List<ProductVariant>> GetVariantsAsync(int productId)
        => await _context.ProductVariants
            .Where(v => v.ProductId == productId && v.IsActive)
            .Include(v => v.Images)
            .ToListAsync();

    public async Task<List<ProductImage>> GetImagesAsync(int productId)
        => await _context.ProductImages
            .Where(i => i.ProductId == productId)
            .OrderBy(i => i.DisplayOrder)
            .ToListAsync();

    public async Task<List<Review>> GetReviewsAsync(int productId, bool approvedOnly = true)
    {
        var query = _context.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId);

        if (approvedOnly)
            query = query.Where(r => r.IsApproved);

        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<bool> DeductStockAsync(int variantId, int quantity)
    {
        var variant = await _context.ProductVariants.FindAsync(variantId);
        if (variant == null || variant.StockQuantity < quantity)
            return false;

        variant.StockQuantity -= quantity;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RestoreStockAsync(int variantId, int quantity)
    {
        var variant = await _context.ProductVariants.FindAsync(variantId);
        if (variant == null)
            return false;

        variant.StockQuantity += quantity;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(List<Product> Items, int TotalCount)> GetAllPaginatedAsync(int page, int pageSize, string? sort = null, int? minRating = null)
    {
        var query = _context.Products.Where(p => !p.Discontinued);
        query = ApplyRatingFilter(query, minRating);
        var totalCount = await query.CountAsync();
        var items = await ApplySort(query, sort)
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.Reviews)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }

    public async Task<(List<Product> Items, int TotalCount)> GetByCategoryPaginatedAsync(int categoryId, int page, int pageSize, string? sort = null, int? minRating = null)
    {
        var query = _context.Products.Where(p => p.Category.Id == categoryId && !p.Discontinued);
        query = ApplyRatingFilter(query, minRating);
        var totalCount = await query.CountAsync();
        var items = await ApplySort(query, sort)
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.Reviews)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }

    public async Task<(List<Product> Items, int TotalCount)> SearchPaginatedAsync(string keyword, int page, int pageSize, string? sort = null, int? minRating = null)
    {
        var lowerKeyword = keyword.ToLower();
        var query = _context.Products
            .Where(p => !p.Discontinued &&
                (p.ProductName.ToLower().Contains(lowerKeyword) ||
                 (p.Category != null && p.Category.CategoryName.ToLower().Contains(lowerKeyword))));
        query = ApplyRatingFilter(query, minRating);
        var totalCount = await query.CountAsync();
        var items = await ApplySort(query, sort)
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.Reviews)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }

    public async Task<(List<Product> Items, int TotalCount)> GetByPriceRangeAsync(int? minPrice, int? maxPrice, int page, int pageSize, string? sort = null, int? minRating = null)
    {
        var query = _context.Products.Where(p => !p.Discontinued);

        if (minPrice.HasValue)
            query = query.Where(p => p.UnitPrice >= minPrice.Value);
        if (maxPrice.HasValue)
            query = query.Where(p => p.UnitPrice <= maxPrice.Value);
        query = ApplyRatingFilter(query, minRating);

        var totalCount = await query.CountAsync();
        var items = await ApplySort(query, sort)
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.Reviews)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }

    public async Task<(List<Product> Items, int TotalCount)> GetByCategoryAndPriceRangeAsync(int categoryId, int? minPrice, int? maxPrice, int page, int pageSize, string? sort = null, int? minRating = null)
    {
        var query = _context.Products.Where(p => p.Category.Id == categoryId && !p.Discontinued);

        if (minPrice.HasValue)
            query = query.Where(p => p.UnitPrice >= minPrice.Value);
        if (maxPrice.HasValue)
            query = query.Where(p => p.UnitPrice <= maxPrice.Value);
        query = ApplyRatingFilter(query, minRating);

        var totalCount = await query.CountAsync();
        var items = await ApplySort(query, sort)
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.Reviews)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }

    private static IQueryable<Product> ApplySort(IQueryable<Product> query, string? sort)
        => sort switch
        {
            "rating_desc" => query
                .OrderByDescending(p => p.Reviews.Where(r => r.IsApproved).Average(r => (double?)r.Rating) ?? 0)
                .ThenByDescending(p => p.Reviews.Count(r => r.IsApproved))
                .ThenBy(p => p.ProductName),
            "price_asc" => query.OrderBy(p => p.UnitPrice).ThenBy(p => p.ProductName),
            "price_desc" => query.OrderByDescending(p => p.UnitPrice).ThenBy(p => p.ProductName),
            "name_desc" => query.OrderByDescending(p => p.ProductName),
            "name_asc" => query.OrderBy(p => p.ProductName),
            _ => query.OrderByDescending(p => p.Id)
        };

    private static IQueryable<Product> ApplyRatingFilter(IQueryable<Product> query, int? minRating)
    {
        if (!minRating.HasValue)
            return query;

        var rating = Math.Clamp(minRating.Value, 1, 5);
        return query.Where(p =>
            p.Reviews.Any(r => r.IsApproved) &&
            p.Reviews.Where(r => r.IsApproved).Average(r => (double?)r.Rating) >= rating);
    }
}
