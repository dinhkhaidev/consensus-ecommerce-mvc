using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebActionResults.Data.Entities;
using WebActionResults.Data.Repositories;
using WebActionResults.Data.Services;
using WebActionResults.ViewModels;
using WebActionResults.Models;

namespace WebActionResults.Controllers;

public class CartController : Controller
{
    public const string CheckoutItemIdsSessionKey = "CHECKOUT_ITEM_IDS";

    private readonly ICartService _cartService;
    private readonly IProductRepository _productRepository;
    private readonly IUserService _userService;
    private readonly ShopDbContext _context;

    public CartController(
        ICartService cartService,
        IProductRepository productRepository,
        IUserService userService,
        ShopDbContext context)
    {
        _cartService = cartService;
        _productRepository = productRepository;
        _userService = userService;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var cartItems = await _cartService.GetCartAsync(userId.Value);
        var subtotal = cartItems.Sum(c => c.UnitPrice * c.Quantity);
        var shippingFee = subtotal > 0 ? 50000 : 0;

        decimal discountAmount = 0;
        string? appliedCouponCode = HttpContext.Session.GetString("COUPON_CODE");
        string? couponDesc = HttpContext.Session.GetString("COUPON_DESC");

        if (!string.IsNullOrEmpty(appliedCouponCode))
        {
            var coupon = _context.Coupons.FirstOrDefault(c => c.Code == appliedCouponCode);
            if (coupon != null && coupon.IsActive && coupon.StartDate <= DateTime.UtcNow && coupon.EndDate >= DateTime.UtcNow)
            {
                if (coupon.Type == CouponType.Percentage)
                {
                    discountAmount = subtotal * (coupon.DiscountValue / 100);
                    if (coupon.MaxDiscountAmount.HasValue && discountAmount > coupon.MaxDiscountAmount.Value)
                        discountAmount = coupon.MaxDiscountAmount.Value;
                }
                else if (coupon.Type == CouponType.FixedAmount)
                {
                    discountAmount = coupon.DiscountValue;
                }
            }
        }

        var viewModel = new CartViewModel
        {
            Items = cartItems.Select(c => new CartItemViewModel
            {
                CartItemId = c.Id,
                ProductId = c.ProductId,
                VariantId = c.VariantId ?? 0,
                ProductName = c.ProductName,
                VariantName = c.VariantName,
                ImageUrl = c.ImageUrl,
                UnitPrice = c.UnitPrice,
                Quantity = c.Quantity,
                PriceAdjustment = c.PriceAdjustment,
                BasePrice = c.BasePrice,
                PriceBreakdown = c.PriceBreakdown
            }).ToList(),
            SubTotal = subtotal,
            ShippingFee = shippingFee,
            DiscountAmount = discountAmount,
            AppliedCouponCode = !string.IsNullOrEmpty(appliedCouponCode) ? $"{(couponDesc ?? appliedCouponCode)} (-{discountAmount.ToString("N0")} VND)" : null,
            Total = subtotal - discountAmount + shippingFee
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckoutSelected(int[] selectedCartItemIds)
    {
        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var selectedIds = selectedCartItemIds?.Distinct().ToList() ?? new List<int>();
        if (!selectedIds.Any())
        {
            TempData["ToastWarning"] = "Please select at least one product to checkout.";
            return RedirectToAction(nameof(Index));
        }

        var cartItems = await _cartService.GetCartAsync(userId.Value);
        var validIds = cartItems
            .Where(i => selectedIds.Contains(i.Id))
            .Select(i => i.Id)
            .ToList();

        if (!validIds.Any())
        {
            TempData["ToastWarning"] = "Selected products are no longer in your cart.";
            return RedirectToAction(nameof(Index));
        }

        HttpContext.Session.SetString(CheckoutItemIdsSessionKey, string.Join(",", validIds));
        return RedirectToAction("Index", "Checkout");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int variantId = 0, int quantity = 1, decimal unitPrice = 0, string? productName = null, string? variantName = null, string? returnUrl = null)
    {
        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var product = await _productRepository.GetByIdWithDetailsAsync(productId);
        if (product == null)
        {
            TempData["ToastError"] = "Product not found.";
            return RedirectToLocal(returnUrl);
        }

        decimal basePrice = product.UnitPrice;
        decimal priceAdjustment = 0;
        string? image = product.Images?.FirstOrDefault()?.ImageUrl;
        string? selectedVariantName = variantName;
        string priceBreakdown = "";
        var activeVariants = product.Variants?.Where(v => v.IsActive).ToList() ?? new List<ProductVariant>();

        if (variantId <= 0 && activeVariants.Any())
        {
            var defaultVariant = activeVariants.FirstOrDefault(v => v.StockQuantity >= quantity);
            if (defaultVariant == null)
            {
                TempData["ToastError"] = "Product is out of stock.";
                return RedirectToLocal(returnUrl);
            }

            variantId = defaultVariant.Id;
        }

        if (variantId > 0)
        {
            var variant = activeVariants.FirstOrDefault(v => v.Id == variantId);
            if (variant != null)
            {
                if (variant.StockQuantity < quantity)
                {
                    TempData["ToastError"] = "Selected variant is out of stock.";
                    return RedirectToLocal(returnUrl);
                }

                basePrice = product.UnitPrice;
                priceAdjustment = variant.PriceAdjustment ?? 0;
                var finalPrice = basePrice + priceAdjustment;
                priceBreakdown = priceAdjustment != 0 ? $"Base: {basePrice:N0} + Variant: +{priceAdjustment:N0} = {finalPrice:N0} VND" : "";

                selectedVariantName = variantName ?? string.Join(" / ", new[] { variant.Color, variant.Size }.Where(v => !string.IsNullOrWhiteSpace(v)));
                if (variant.Images != null && variant.Images.Any())
                    image = variant.Images.First().ImageUrl;
            }
        }

        await _cartService.AddToCartAsync(userId.Value, productId, variantId, unitPrice > 0 ? unitPrice : (basePrice + priceAdjustment), basePrice, priceAdjustment, priceBreakdown, product.ProductName, selectedVariantName, image, quantity);

        TempData["ToastSuccess"] = "Added to cart.";
        return RedirectToLocal(returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int productId, int variantId, int quantity)
    {
        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        await _cartService.UpdateQuantityAsync(userId.Value, productId, variantId, quantity);
        TempData["ToastSuccess"] = "Cart updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int productId, int variantId = 0)
    {
        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        await _cartService.RemoveFromCartAsync(userId.Value, productId, variantId);
        TempData["ToastSuccess"] = "Removed from cart.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        await _cartService.ClearCartAsync(userId.Value);
        HttpContext.Session.Remove("COUPON_CODE");
        HttpContext.Session.Remove("COUPON_TYPE");
        HttpContext.Session.Remove("COUPON_VALUE");
        HttpContext.Session.Remove("COUPON_DESC");
        TempData["ToastSuccess"] = "Cart cleared.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveCoupon()
    {
        HttpContext.Session.Remove("COUPON_CODE");
        HttpContext.Session.Remove("COUPON_TYPE");
        HttpContext.Session.Remove("COUPON_VALUE");
        HttpContext.Session.Remove("COUPON_DESC");
        TempData["ToastSuccess"] = "Coupon removed.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyCoupon(string couponCode)
    {
        if (string.IsNullOrWhiteSpace(couponCode))
        {
            TempData["ToastError"] = "Please enter a coupon code.";
            return RedirectToAction(nameof(Index));
        }

        var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == couponCode);
        if (coupon == null)
        {
            TempData["ToastError"] = "Invalid coupon code.";
            return RedirectToAction(nameof(Index));
        }

        var now = DateTime.UtcNow;
        if (!coupon.IsActive || coupon.StartDate > now || coupon.EndDate < now)
        {
            TempData["ToastError"] = "This coupon has expired or is not active.";
            return RedirectToAction(nameof(Index));
        }

        if (coupon.UsedCount >= coupon.UsageLimit)
        {
            TempData["ToastError"] = "This coupon has reached its usage limit.";
            return RedirectToAction(nameof(Index));
        }

        HttpContext.Session.SetString("COUPON_CODE", coupon.Code);
        HttpContext.Session.SetString("COUPON_TYPE", ((int)coupon.Type).ToString());
        HttpContext.Session.SetString("COUPON_VALUE", coupon.DiscountValue.ToString());
        HttpContext.Session.SetString("COUPON_DESC", coupon.Description ?? coupon.Code);

        TempData["ToastSuccess"] = $"Coupon applied: {coupon.Code} - {coupon.Description}";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetCartCount()
    {
        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == null)
            return Json(new { count = 0 });

        var cartItems = await _cartService.GetCartAsync(userId.Value);
        var count = cartItems.Sum(c => c.Quantity);
        return Json(new { count });
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction(nameof(Index));
    }
}
