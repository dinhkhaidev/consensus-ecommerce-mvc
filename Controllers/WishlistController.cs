using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebActionResults.Data.Services;
using WebActionResults.Utilities;
using WebActionResults.ViewModels;

namespace WebActionResults.Controllers;

[Authorize]
public class WishlistController : Controller
{
    private readonly IWishlistService _wishlistService;
    private readonly IUserService _userService;
    private readonly IWebSettingsService _settingsService;

    public WishlistController(IWishlistService wishlistService, IUserService userService, IWebSettingsService settingsService)
    {
        _wishlistService = wishlistService;
        _userService = userService;
        _settingsService = settingsService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var wishlists = await _wishlistService.GetUserWishlistAsync(userId.Value);
        var viewModels = wishlists.Select(w => new WishlistViewModel
        {
            Id = w.Id,
            ProductId = w.ProductId,
            ProductName = w.Product.ProductName,
            UnitPrice = w.Product.UnitPrice,
            MainImageUrl = w.Product.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl,
            AddedAt = w.CreatedAt
        }).ToList();

        return View(viewModels);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, string? returnUrl = null)
    {
        var settings = await _settingsService.GetAllSettingsAsync();
        if (!string.Equals(settings.GetValueOrDefault("EnableWishlist", "true"), "true", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ToastError"] = "Wishlist is currently disabled.";
            return RedirectToAction("Index", "Product");
        }

        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        await _wishlistService.AddToWishlistAsync(userId.Value, productId);
        TempData["ToastSuccess"] = "Added to wishlist.";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(RedirectUrlSanitizer.EscapeHeaderValue(returnUrl));

        return RedirectToAction("Index", "Product");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int productId)
    {
        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        await _wishlistService.RemoveFromWishlistAsync(userId.Value, productId);
        TempData["ToastSuccess"] = "Removed from wishlist.";

        return RedirectToAction(nameof(Index));
    }
}
