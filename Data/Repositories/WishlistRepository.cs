using Microsoft.EntityFrameworkCore;
using WebActionResults.Data.Entities;
using WebActionResults.Models;

namespace WebActionResults.Data.Repositories;

public interface IWishlistRepository
{
    Task<List<Wishlist>> GetByUserIdAsync(int userId);
    Task<Wishlist?> GetByUserAndProductAsync(int userId, int productId);
    Task AddAsync(Wishlist wishlist);
    Task RemoveAsync(int userId, int productId);
    Task<bool> ExistsAsync(int userId, int productId);
}

public class WishlistRepository : IWishlistRepository
{
    private readonly ShopDbContext _context;

    public WishlistRepository(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<List<Wishlist>> GetByUserIdAsync(int userId)
        => await _context.Wishlists
            .Where(w => w.UserId == userId)
            .Include(w => w.Product)
                .ThenInclude(p => p.Images.Where(i => i.IsMain).Take(1))
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

    public async Task<Wishlist?> GetByUserAndProductAsync(int userId, int productId)
        => await _context.Wishlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

    public async Task AddAsync(Wishlist wishlist)
    {
        _context.Wishlists.Add(wishlist);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(int userId, int productId)
    {
        var item = await GetByUserAndProductAsync(userId, productId);
        if (item != null)
        {
            _context.Wishlists.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int userId, int productId)
        => await _context.Wishlists.AnyAsync(w => w.UserId == userId && w.ProductId == productId);
}