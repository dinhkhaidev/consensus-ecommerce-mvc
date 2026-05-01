using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebActionResults.Extensions;
using WebActionResults.Models;

namespace WebActionResults.Controllers;

public class CartController : Controller
{
    private const string CartSessionKey = "CART_SESSION";
    private readonly ShopDbContext _context;

    public CartController(ShopDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View(GetCart());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1, string? returnUrl = null)
    {
        if (quantity < 1)
        {
            quantity = 1;
        }

        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductID == productId);

        if (product == null)
        {
            TempData["ErrorMessage"] = "Product not found.";
            return RedirectToLocal(returnUrl);
        }

        var cart = GetCart();
        var existingItem = cart.FirstOrDefault(item => item.ProductId == productId);

        if (existingItem == null)
        {
            cart.Add(new CartItem
            {
                ProductId = product.ProductID,
                ProductName = product.ProductName,
                UnitPrice = product.UnitPrice,
                Quantity = quantity
            });
        }
        else
        {
            existingItem.Quantity += quantity;
        }

        SaveCart(cart);
        TempData["SuccessMessage"] = "Added product to cart.";

        return RedirectToLocal(returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Update(int productId, int quantity)
    {
        var cart = GetCart();
        var existingItem = cart.FirstOrDefault(item => item.ProductId == productId);

        if (existingItem == null)
        {
            TempData["ErrorMessage"] = "Cart item not found.";
            return RedirectToAction(nameof(Index));
        }

        if (quantity <= 0)
        {
            cart.Remove(existingItem);
        }
        else
        {
            existingItem.Quantity = quantity;
        }

        SaveCart(cart);
        TempData["SuccessMessage"] = "Cart updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int productId)
    {
        var cart = GetCart();
        var item = cart.FirstOrDefault(x => x.ProductId == productId);

        if (item != null)
        {
            cart.Remove(item);
            SaveCart(cart);
            TempData["SuccessMessage"] = "Removed item from cart.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        HttpContext.Session.Remove(CartSessionKey);
        TempData["SuccessMessage"] = "Cart cleared.";
        return RedirectToAction(nameof(Index));
    }

    private List<CartItem> GetCart()
    {
        return HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
    }

    private void SaveCart(List<CartItem> cart)
    {
        HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }
}