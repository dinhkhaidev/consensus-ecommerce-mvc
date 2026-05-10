using Microsoft.EntityFrameworkCore;
using WebActionResults.Data.Entities;
using WebActionResults.Models;

namespace WebActionResults.Data.Repositories;

public interface ICartRepository
{
    Task<Cart?> GetCartByUserIdAsync(int userId);
    Task<Cart> GetOrCreateCartAsync(int userId);
    Task<CartItem> AddItemAsync(int cartId, int productId, int variantId, decimal unitPrice, decimal basePrice, decimal priceAdjustment, string priceBreakdown, string productName, string? variantName, string? imageUrl, int quantity);
    Task UpdateItemQuantityAsync(int itemId, int quantity);
    Task RemoveItemAsync(int itemId);
    Task ClearCartAsync(int cartId);
    Task<CartItem?> GetItemAsync(int cartId, int productId, int variantId);
}

public class CartRepository : ICartRepository
{
    private readonly ShopDbContext _context;

    public CartRepository(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetCartByUserIdAsync(int userId)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Cart> GetOrCreateCartAsync(int userId)
    {
        var cart = await GetCartByUserIdAsync(userId);
        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }
        return cart;
    }

    public async Task<CartItem> AddItemAsync(int cartId, int productId, int variantId, decimal unitPrice, decimal basePrice, decimal priceAdjustment, string priceBreakdown, string productName, string? variantName, string? imageUrl, int quantity)
    {
        // Check if item already exists
        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(i => i.CartId == cartId && i.ProductId == productId && i.VariantId == (variantId > 0 ? variantId : null));

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
            await _context.SaveChangesAsync();
            return existingItem;
        }

        var item = new CartItem
        {
            CartId = cartId,
            ProductId = productId,
            VariantId = variantId > 0 ? variantId : null,
            ProductName = productName,
            VariantName = variantName,
            ImageUrl = imageUrl,
            UnitPrice = unitPrice,
            BasePrice = basePrice,
            PriceAdjustment = priceAdjustment,
            PriceBreakdown = priceBreakdown,
            Quantity = quantity
        };
        _context.CartItems.Add(item);

        var cart = await _context.Carts.FindAsync(cartId);
        if (cart != null)
            cart.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return item;
    }

    public async Task UpdateItemQuantityAsync(int itemId, int quantity)
    {
        var item = await _context.CartItems.FindAsync(itemId);
        if (item != null)
        {
            if (quantity <= 0)
            {
                _context.CartItems.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveItemAsync(int itemId)
    {
        var item = await _context.CartItems.FindAsync(itemId);
        if (item != null)
        {
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ClearCartAsync(int cartId)
    {
        var items = await _context.CartItems.Where(i => i.CartId == cartId).ToListAsync();
        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync();
    }

    public async Task<CartItem?> GetItemAsync(int cartId, int productId, int variantId)
    {
        return await _context.CartItems
            .FirstOrDefaultAsync(i => i.CartId == cartId && i.ProductId == productId && i.VariantId == (variantId > 0 ? variantId : null));
    }
}