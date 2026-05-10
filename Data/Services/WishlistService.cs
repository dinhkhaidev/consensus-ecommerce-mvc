using WebActionResults.Data.Entities;
using WebActionResults.Data.Repositories;

namespace WebActionResults.Data.Services;

public interface IWishlistService
{
    Task<List<Wishlist>> GetUserWishlistAsync(int userId);
    Task<bool> AddToWishlistAsync(int userId, int productId);
    Task<bool> RemoveFromWishlistAsync(int userId, int productId);
    Task<bool> IsInWishlistAsync(int userId, int productId);
}

public class WishlistService : IWishlistService
{
    private readonly IWishlistRepository _wishlistRepository;

    public WishlistService(IWishlistRepository wishlistRepository)
    {
        _wishlistRepository = wishlistRepository;
    }

    public async Task<List<Wishlist>> GetUserWishlistAsync(int userId)
        => await _wishlistRepository.GetByUserIdAsync(userId);

    public async Task<bool> AddToWishlistAsync(int userId, int productId)
    {
        if (await _wishlistRepository.ExistsAsync(userId, productId))
            return true;

        var wishlist = new Wishlist
        {
            UserId = userId,
            ProductId = productId,
            CreatedAt = DateTime.UtcNow
        };

        await _wishlistRepository.AddAsync(wishlist);
        return true;
    }

    public async Task<bool> RemoveFromWishlistAsync(int userId, int productId)
    {
        await _wishlistRepository.RemoveAsync(userId, productId);
        return true;
    }

    public async Task<bool> IsInWishlistAsync(int userId, int productId)
        => await _wishlistRepository.ExistsAsync(userId, productId);
}