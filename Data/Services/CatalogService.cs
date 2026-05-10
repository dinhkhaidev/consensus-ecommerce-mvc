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
    Task<List<Product>> GetFeaturedProductsAsync(int count = 8);
    Task<List<Product>> SearchProductsAsync(string keyword);
    Task<(List<Product> Items, int TotalCount)> GetProductsPaginatedAsync(int page, int pageSize);
    Task<(List<Product> Items, int TotalCount)> GetProductsByCategoryPaginatedAsync(int categoryId, int page, int pageSize);
    Task<(List<Product> Items, int TotalCount)> SearchProductsPaginatedAsync(string keyword, int page, int pageSize);
    Task<(List<Product> Items, int TotalCount)> GetProductsByPriceRangeAsync(int? minPrice, int? maxPrice, int page, int pageSize);
    Task<(List<Product> Items, int TotalCount)> GetProductsByCategoryAndPriceRangeAsync(int categoryId, int? minPrice, int? maxPrice, int page, int pageSize);

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

    public async Task<(List<Product> Items, int TotalCount)> GetProductsPaginatedAsync(int page, int pageSize)
        => await _productRepository.GetAllPaginatedAsync(page, pageSize);

    public async Task<(List<Product> Items, int TotalCount)> GetProductsByCategoryPaginatedAsync(int categoryId, int page, int pageSize)
        => await _productRepository.GetByCategoryPaginatedAsync(categoryId, page, pageSize);

    public async Task<(List<Product> Items, int TotalCount)> SearchProductsPaginatedAsync(string keyword, int page, int pageSize)
        => await _productRepository.SearchPaginatedAsync(keyword, page, pageSize);

    public async Task<(List<Product> Items, int TotalCount)> GetProductsByPriceRangeAsync(int? minPrice, int? maxPrice, int page, int pageSize)
        => await _productRepository.GetByPriceRangeAsync(minPrice, maxPrice, page, pageSize);

    public async Task<(List<Product> Items, int TotalCount)> GetProductsByCategoryAndPriceRangeAsync(int categoryId, int? minPrice, int? maxPrice, int page, int pageSize)
        => await _productRepository.GetByCategoryAndPriceRangeAsync(categoryId, minPrice, maxPrice, page, pageSize);
}