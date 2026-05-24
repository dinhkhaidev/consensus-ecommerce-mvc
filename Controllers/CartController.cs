using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebActionResults.Data.Entities;
using WebActionResults.Data.Repositories;
using WebActionResults.Data.Services;
using WebActionResults.ViewModels;
using WebActionResults.Models;
using WebActionResults.Utilities;

namespace WebActionResults.Controllers;

public class CartController : Controller
{
    public const string CheckoutItemIdsSessionKey = "CHECKOUT_ITEM_IDS";
    private const string PendingCartAddSessionKey = "PENDING_CART_ADD";

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
        {
            SavePendingCartAdd(productId, variantId, quantity, variantName, returnUrl);
            TempData["ToastInfo"] = "Đăng nhập xong mình sẽ thêm sản phẩm này vào giỏ cho bạn.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(CompletePendingAdd), "Cart") });
        }

        var result = await AddProductToCartForUserAsync(userId.Value, productId, variantId, quantity, variantName);
        if (!result.Success)
        {
            TempData["ToastError"] = result.Message;
            return RedirectToLocal(returnUrl);
        }

        TempData["ToastSuccess"] = result.Message;
        return RedirectToLocal(returnUrl);
    }

    [HttpGet]
    public async Task<IActionResult> CompletePendingAdd()
    {
        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == null)
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(CompletePendingAdd), "Cart") });

        var pending = ReadPendingCartAdd();
        if (pending == null)
            return RedirectToAction(nameof(Index));

        HttpContext.Session.Remove(PendingCartAddSessionKey);

        var result = await AddProductToCartForUserAsync(userId.Value, pending.ProductId, pending.VariantId, pending.Quantity, pending.VariantName);
        if (result.Success)
            TempData["ToastSuccess"] = result.Message;
        else
            TempData["ToastError"] = result.Message;

        return RedirectToLocal(pending.ReturnUrl);
    }

    private async Task<CartAddResult> AddProductToCartForUserAsync(int userId, int productId, int variantId, int quantity, string? variantName)
    {
        quantity = Math.Clamp(quantity, 1, 99);

        var product = await _productRepository.GetByIdWithDetailsAsync(productId);
        if (product == null)
            return new CartAddResult(false, "Product not found.");

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
                return new CartAddResult(false, "Product is out of stock.");

            variantId = defaultVariant.Id;
        }

        if (variantId > 0)
        {
            var variant = activeVariants.FirstOrDefault(v => v.Id == variantId);
            if (variant != null)
            {
                if (variant.StockQuantity < quantity)
                    return new CartAddResult(false, "Selected variant is out of stock.");

                basePrice = product.UnitPrice;
                priceAdjustment = variant.PriceAdjustment ?? 0;
                var finalPrice = basePrice + priceAdjustment;
                priceBreakdown = priceAdjustment != 0 ? $"Base: {basePrice:N0} + Variant: +{priceAdjustment:N0} = {finalPrice:N0} VND" : "";

                selectedVariantName = variantName ?? string.Join(" / ", new[] { variant.Color, variant.Size }.Where(v => !string.IsNullOrWhiteSpace(v)));
                if (variant.Images != null && variant.Images.Any())
                    image = variant.Images.First().ImageUrl;
            }
        }

        var finalUnitPriceVnd = basePrice + priceAdjustment;
        await _cartService.AddToCartAsync(userId, productId, variantId, finalUnitPriceVnd, basePrice, priceAdjustment, priceBreakdown, product.ProductName, selectedVariantName, image, quantity);

        return new CartAddResult(true, "Added to cart.");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRoomItems([FromBody] RoomCartRequest? request)
    {
        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == null)
            return Unauthorized(new { success = false, loginUrl = Url.Action("Login", "Account", new { returnUrl = Url.Action("Index", "Room3D") }) });

        var requestedItems = (request?.Items ?? new List<RoomCartItemRequest>())
            .Where(i => !string.IsNullOrWhiteSpace(i.ProductId))
            .Select(i =>
            {
                var roomProductId = i.ProductId!.Trim();
                var productName = Room3DProductCatalog.ProductNamesByRoomId.TryGetValue(roomProductId, out var mappedName)
                    ? mappedName
                    : i.Name?.Trim();

                return new RoomCartResolvedItem(
                    productName ?? string.Empty,
                    GetCatalogCategoryName(i.Category),
                    i.Price > 0 ? i.Price : 1000000,
                    Math.Clamp(i.Quantity, 1, 10));
            })
            .Where(i => !string.IsNullOrWhiteSpace(i.ProductName))
            .GroupBy(i => i.ProductName, StringComparer.OrdinalIgnoreCase)
            .Select(g => new RoomCartResolvedItem(
                g.Key,
                g.First().CategoryName,
                g.First().Price,
                Math.Clamp(g.Sum(i => i.Quantity), 1, 10)))
            .ToList();

        if (!requestedItems.Any())
            return BadRequest(new { success = false, message = "Chua co mon nao trong phong 3D duoc gan voi san pham that." });

        var addedProductIds = new List<int>();
        var skippedProducts = new List<int>();

        foreach (var item in requestedItems)
        {
            var product = await GetOrCreateProductByNameWithDetailsAsync(item.ProductName, item.CategoryName, item.Price);
            if (product == null || product.Discontinued)
            {
                skippedProducts.Add(0);
                continue;
            }

            var added = await AddCatalogProductToCartAsync(userId.Value, product, item.Quantity);
            if (added)
                addedProductIds.Add(product.Id);
            else
                skippedProducts.Add(product.Id);
        }

        if (!addedProductIds.Any())
            return BadRequest(new { success = false, message = "Cac san pham phong 3D hien het hang hoac khong ton tai." });

        HttpContext.Session.Remove(CheckoutItemIdsSessionKey);
        TempData["ToastSuccess"] = "Da them bo phong 3D vao gio hang.";

        return Json(new
        {
            success = true,
            addedCount = addedProductIds.Count,
            skippedCount = skippedProducts.Count,
            redirectUrl = Url.Action("Index", "Cart")
        });
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
            return Redirect(RedirectUrlSanitizer.EscapeHeaderValue(returnUrl));
        return RedirectToAction(nameof(Index));
    }

    private void SavePendingCartAdd(int productId, int variantId, int quantity, string? variantName, string? returnUrl)
    {
        var safeReturnUrl = !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? RedirectUrlSanitizer.EscapeHeaderValue(returnUrl)
            : Url.Action("Details", "Product", new { id = productId });

        var pending = new PendingCartAdd(
            productId,
            Math.Max(0, variantId),
            Math.Clamp(quantity, 1, 99),
            string.IsNullOrWhiteSpace(variantName) ? null : variantName.Trim(),
            safeReturnUrl);

        HttpContext.Session.SetString(PendingCartAddSessionKey, System.Text.Json.JsonSerializer.Serialize(pending));
    }

    private PendingCartAdd? ReadPendingCartAdd()
    {
        var raw = HttpContext.Session.GetString(PendingCartAddSessionKey);
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<PendingCartAdd>(raw);
        }
        catch
        {
            HttpContext.Session.Remove(PendingCartAddSessionKey);
            return null;
        }
    }

    private async Task<bool> AddCatalogProductToCartAsync(int userId, Product product, int quantity)
    {
        decimal basePrice = product.UnitPrice;
        decimal priceAdjustment = 0;
        string? image = product.Images?
            .OrderByDescending(i => i.IsMain)
            .ThenBy(i => i.DisplayOrder)
            .FirstOrDefault()
            ?.ImageUrl;
        string? selectedVariantName = null;
        string priceBreakdown = "";
        var variantId = 0;
        var activeVariants = product.Variants?.Where(v => v.IsActive).ToList() ?? new List<ProductVariant>();

        if (activeVariants.Any())
        {
            var defaultVariant = activeVariants.FirstOrDefault(v => v.StockQuantity >= quantity);
            if (defaultVariant == null)
            {
                defaultVariant = activeVariants.FirstOrDefault(v => v.SKU == $"ROOM3D-{product.Id}");
                if (defaultVariant == null)
                {
                    defaultVariant = new ProductVariant
                    {
                        ProductId = product.Id,
                        Size = "Default",
                        Color = "Room 3D",
                        SKU = $"ROOM3D-{product.Id}",
                        PriceAdjustment = 0,
                        StockQuantity = 100,
                        IsActive = true
                    };
                    _context.ProductVariants.Add(defaultVariant);
                }
                else
                {
                    defaultVariant.StockQuantity = Math.Max(defaultVariant.StockQuantity, 100);
                }

                await _context.SaveChangesAsync();
            }

            variantId = defaultVariant.Id;
            priceAdjustment = defaultVariant.PriceAdjustment ?? 0;
            selectedVariantName = string.Join(" / ", new[] { defaultVariant.Color, defaultVariant.Size }.Where(v => !string.IsNullOrWhiteSpace(v)));
            if (defaultVariant.Images != null && defaultVariant.Images.Any())
                image = defaultVariant.Images.OrderBy(i => i.DisplayOrder).First().ImageUrl;
        }

        var finalPrice = basePrice + priceAdjustment;
        if (priceAdjustment != 0)
            priceBreakdown = $"Base: {basePrice:N0} + Variant: +{priceAdjustment:N0} = {finalPrice:N0} VND";

        await _cartService.AddToCartAsync(
            userId,
            product.Id,
            variantId,
            finalPrice,
            basePrice,
            priceAdjustment,
            priceBreakdown,
            product.ProductName,
            selectedVariantName,
            image,
            quantity);

        return true;
    }

    private async Task<Product?> GetOrCreateProductByNameWithDetailsAsync(string productName, string categoryName, decimal price)
    {
        var normalizedName = productName.Trim().ToLower();
        var existingProduct = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Images)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.ProductName.ToLower() == normalizedName);

        if (existingProduct != null)
            return existingProduct;

        var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryName == categoryName);
        if (category == null)
        {
            category = new Category
            {
                CategoryName = categoryName,
                Description = $"Auto-created for Room 3D {categoryName.ToLower()} items",
                CreatedAt = DateTime.UtcNow
            };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        var product = new Product
        {
            ProductName = productName.Trim(),
            CategoryID = category.Id,
            QuantityPerUnit = "1 piece",
            UnitPrice = price > 0 ? price : 1000000,
            Discontinued = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Images)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == product.Id);
    }

    private static string GetCatalogCategoryName(string? roomCategory)
    {
        return (roomCategory ?? "").Trim().ToLowerInvariant() switch
        {
            "decor" or "plant" or "rug" => "Decor",
            "table" or "chair" or "sofa" or "lamp" or "cabinet" or "shelf" => "Living Room",
            _ => "Living Room"
        };
    }

    public sealed class RoomCartRequest
    {
        public List<RoomCartItemRequest> Items { get; set; } = new();
    }

    public sealed class RoomCartItemRequest
    {
        public string? ProductId { get; set; }
        public string? Name { get; set; }
        public string? Category { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; } = 1;
    }

    private sealed record PendingCartAdd(int ProductId, int VariantId, int Quantity, string? VariantName, string? ReturnUrl);
    private sealed record CartAddResult(bool Success, string Message);
    private sealed record RoomCartResolvedItem(string ProductName, string CategoryName, decimal Price, int Quantity);
}
