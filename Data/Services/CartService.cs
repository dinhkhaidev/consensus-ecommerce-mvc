using WebActionResults.Data.Entities;
using WebActionResults.Data.Repositories;

namespace WebActionResults.Data.Services;

public interface ICartService
{
    Task<List<CartItem>> GetCartAsync(int userId);
    Task AddToCartAsync(int userId, int productId, int variantId, decimal unitPrice, decimal basePrice, decimal priceAdjustment, string priceBreakdown, string productName, string? variantName = null, string? imageUrl = null, int quantity = 1);
    Task UpdateQuantityAsync(int userId, int productId, int variantId, int quantity);
    Task RemoveFromCartAsync(int userId, int productId, int variantId);
    Task RemoveItemsAsync(int userId, IEnumerable<int> cartItemIds);
    Task RemovePurchasedItemsAsync(int userId, IEnumerable<OrderItem> orderItems);
    Task ClearCartAsync(int userId);
    decimal GetCartTotal(List<CartItem> items);
}

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;

    public CartService(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<List<CartItem>> GetCartAsync(int userId)
    {
        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        return cart?.Items.ToList() ?? new List<CartItem>();
    }

    public async Task AddToCartAsync(int userId, int productId, int variantId, decimal unitPrice, decimal basePrice, decimal priceAdjustment, string priceBreakdown, string productName, string? variantName = null, string? imageUrl = null, int quantity = 1)
    {
        var cart = await _cartRepository.GetOrCreateCartAsync(userId);
        await _cartRepository.AddItemAsync(cart.Id, productId, variantId, unitPrice, basePrice, priceAdjustment, priceBreakdown, productName, variantName, imageUrl, quantity);
    }

    public async Task UpdateQuantityAsync(int userId, int productId, int variantId, int quantity)
    {
        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        if (cart == null) return;

        var item = await _cartRepository.GetItemAsync(cart.Id, productId, variantId);
        if (item != null)
        {
            await _cartRepository.UpdateItemQuantityAsync(item.Id, quantity);
        }
    }

    public async Task RemoveFromCartAsync(int userId, int productId, int variantId)
    {
        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        if (cart == null) return;

        var item = await _cartRepository.GetItemAsync(cart.Id, productId, variantId);
        if (item != null)
        {
            await _cartRepository.RemoveItemAsync(item.Id);
        }
    }

    public async Task RemoveItemsAsync(int userId, IEnumerable<int> cartItemIds)
    {
        var selectedIds = cartItemIds.Distinct().ToHashSet();
        if (!selectedIds.Any()) return;

        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        if (cart == null) return;

        foreach (var item in cart.Items.Where(i => selectedIds.Contains(i.Id)).ToList())
        {
            await _cartRepository.RemoveItemAsync(item.Id);
        }
    }

    public async Task RemovePurchasedItemsAsync(int userId, IEnumerable<OrderItem> orderItems)
    {
        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        if (cart == null) return;

        foreach (var orderItem in orderItems)
        {
            var cartItem = cart.Items.FirstOrDefault(i =>
                i.ProductId == orderItem.ProductId &&
                i.VariantId == orderItem.VariantId);

            if (cartItem != null)
                await _cartRepository.RemoveItemAsync(cartItem.Id);
        }
    }

    public async Task ClearCartAsync(int userId)
    {
        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        if (cart == null) return;

        await _cartRepository.ClearCartAsync(cart.Id);
    }

    public decimal GetCartTotal(List<CartItem> items)
    {
        return items.Sum(c => c.UnitPrice * c.Quantity);
    }
}
