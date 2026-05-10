using Microsoft.EntityFrameworkCore;
using WebActionResults.Data.Entities;
using WebActionResults.Models;

namespace WebActionResults.Data.Repositories;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<List<Category>> GetWithProductsAsync();
    Task<List<Category>> GetActiveCategoriesAsync();
    Task<Category?> GetByIdWithProductsAsync(int id);
}

public class CategoryRepository : ICategoryRepository
{
    private readonly ShopDbContext _context;

    public CategoryRepository(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllAsync()
        => await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();

    public async Task<Category?> GetByIdAsync(int id)
        => await _context.Categories.FindAsync(id);

    public async Task<List<Category>> GetWithProductsAsync()
        => await _context.Categories
            .Include(c => c.Products)
            .OrderBy(c => c.CategoryName)
            .ToListAsync();

    public async Task<List<Category>> GetActiveCategoriesAsync()
        => await _context.Categories
            .Where(c => c.Products.Any(p => !p.Discontinued))
            .OrderBy(c => c.CategoryName)
            .ToListAsync();

    public async Task<Category?> GetByIdWithProductsAsync(int id)
        => await _context.Categories
            .Include(c => c.Products.Where(p => !p.Discontinued))
            .FirstOrDefaultAsync(c => c.Id == id);
}