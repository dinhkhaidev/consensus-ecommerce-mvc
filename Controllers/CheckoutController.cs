using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebActionResults.Data.Entities;
using WebActionResults.Data.Repositories;
using WebActionResults.Data.Services;
using WebActionResults.ViewModels;
using WebActionResults.Models;

namespace WebActionResults.Controllers;

[Authorize]
public class CheckoutController : Controller
{
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;
    private readonly IUserService _userService;
    private readonly ICouponRepository _couponRepository;
    private readonly IWebSettingsService _settingsService;

    public CheckoutController(
        ICartService cartService,
        IOrderService orderService,
        IUserService userService,
        ICouponRepository couponRepository,
        IWebSettingsService settingsService)
    {
        _cartService = cartService;
        _orderService = orderService;
        _userService = userService;
        _couponRepository = couponRepository;
        _settingsService = settingsService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var cartItems = GetSelectedCartItems(await _cartService.GetCartAsync(userId.Value));
        if (!cartItems.Any())
        {
            HttpContext.Session.Remove(CartController.CheckoutItemIdsSessionKey);
            return RedirectToAction("Index", "Cart");
        }

        var user = await _userService.GetByIdAsync(userId.Value);
        if (user?.Birthday == null || !IsAtLeast16(user.Birthday.Value))
        {
            TempData["ToastWarningKey"] = "Auth.MustBe16ToPurchase";
            return RedirectToAction("Profile", "Account");
        }

        var addresses = await _userService.GetAddressesAsync(userId.Value);
        var defaultAddress = addresses.FirstOrDefault(a => a.IsDefault) ?? addresses.FirstOrDefault();
        var addressViewModels = addresses.Select(AddressViewModel.FromEntity).ToList();
        var subtotal = cartItems.Sum(c => c.UnitPrice * c.Quantity);

        // Get shipping settings from admin
        var allSettings = await _settingsService.GetAllSettingsAsync();
        decimal configuredShippingFee = decimal.TryParse(allSettings.GetValueOrDefault("StandardShippingFee", "50000"), out var sf) ? sf : 50000;
        decimal freeShippingThreshold = decimal.TryParse(allSettings.GetValueOrDefault("FreeShippingThreshold", "500000"), out var ft) ? ft : 500000;

        // Get coupon from session (applied in cart)
        string? appliedCouponCode = HttpContext.Session.GetString("COUPON_CODE");
        decimal discountAmount = 0;
        decimal shippingFee = subtotal >= freeShippingThreshold ? 0 : configuredShippingFee;

        if (!string.IsNullOrEmpty(appliedCouponCode))
        {
            var coupon = await _couponRepository.GetByCodeAsync(appliedCouponCode);
            if (coupon != null &&
                coupon.IsActive &&
                coupon.StartDate <= DateTime.UtcNow &&
                coupon.EndDate >= DateTime.UtcNow &&
                coupon.UsedCount < coupon.UsageLimit &&
                (!coupon.MinOrderAmount.HasValue || subtotal >= coupon.MinOrderAmount.Value))
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
                else if (coupon.Type == CouponType.FreeShipping)
                {
                    shippingFee = 0;
                }
            }
        }

        var viewModel = new CheckoutViewModel
        {
            Items = cartItems.Select(c => new CartItemViewModel
            {
                CartItemId = c.Id,
                ProductId = c.ProductId,
                VariantId = c.VariantId,
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
            CouponCode = appliedCouponCode,
            Total = subtotal - discountAmount + shippingFee,
            SelectedAddressId = defaultAddress?.Id,
            AddressEntryMode = defaultAddress != null ? "saved" : "manual",
            SaveNewAddressAsDefault = !addresses.Any(),
            SelectedAddress = defaultAddress != null ? new AddressViewModel
            {
                Id = defaultAddress.Id,
                FullName = defaultAddress.FullName,
                Phone = defaultAddress.Phone,
                AddressLine = defaultAddress.AddressLine,
                Ward = defaultAddress.Ward,
                District = defaultAddress.District,
                City = defaultAddress.City
            } : new AddressViewModel
            {
                FullName = user?.FullName ?? string.Empty,
                Phone = user?.Phone ?? string.Empty
            }
        };

        ViewBag.Addresses = addressViewModels;

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessCheckout(CheckoutViewModel model)
    {
        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var user = await _userService.GetByIdAsync(userId.Value);
        if (user?.Birthday == null || !IsAtLeast16(user.Birthday.Value))
        {
            TempData["ToastWarningKey"] = "Auth.MustBe16ToPurchase";
            return RedirectToAction("Profile", "Account");
        }

        var cartItems = GetSelectedCartItems(await _cartService.GetCartAsync(userId.Value));
        if (!cartItems.Any())
            return RedirectToAction("Index", "Cart");

        var addresses = await _userService.GetAddressesAsync(userId.Value);
        Address? address = null;
        var useSavedAddress = model.AddressEntryMode != "manual" && model.SelectedAddressId.HasValue;

        if (useSavedAddress)
        {
            address = addresses.FirstOrDefault(a => a.Id == model.SelectedAddressId);
        }

        if (useSavedAddress && address == null)
        {
            ModelState.AddModelError(nameof(model.SelectedAddressId), "Please select a valid shipping address.");
        }
        else if (!useSavedAddress)
        {
            if (model.SelectedAddress == null ||
                string.IsNullOrWhiteSpace(model.SelectedAddress.FullName) ||
                string.IsNullOrWhiteSpace(model.SelectedAddress.Phone) ||
                string.IsNullOrWhiteSpace(model.SelectedAddress.AddressLine) ||
                string.IsNullOrWhiteSpace(model.SelectedAddress.Ward) ||
                string.IsNullOrWhiteSpace(model.SelectedAddress.District) ||
                string.IsNullOrWhiteSpace(model.SelectedAddress.City))
            {
                ModelState.AddModelError("", "Please fill in all required address fields.");
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(model.SelectedAddress.Phone, @"^0[0-9]{9,10}$"))
            {
                ModelState.AddModelError("SelectedAddress.Phone", "Invalid phone number (10-11 digits starting with 0).");
            }
        }

        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Index));
        }

        if (!useSavedAddress && model.SelectedAddress != null)
        {
            address = new Address
            {
                UserId = userId.Value,
                FullName = model.SelectedAddress.FullName.Trim(),
                Phone = model.SelectedAddress.Phone.Trim(),
                AddressLine = model.SelectedAddress.AddressLine.Trim(),
                Ward = model.SelectedAddress.Ward?.Trim(),
                District = model.SelectedAddress.District?.Trim(),
                City = model.SelectedAddress.City?.Trim(),
                PostalCode = model.SelectedAddress.PostalCode ?? string.Empty,
                IsDefault = !addresses.Any() || model.SaveNewAddressAsDefault
            };

            var addResult = await _userService.AddAddressAsync(address);
            if (!addResult.Succeeded)
            {
                TempData["ToastError"] = "Failed to save shipping address.";
                return RedirectToAction(nameof(Index));
            }
        }

        Order order;
        try
        {
            order = await _orderService.CreateOrderFromCartAsync(
                userId.Value,
                address?.Id,
                cartItems,
                model.CouponCode);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ToastError"] = ex.Message;
            return RedirectToAction("Index", "Cart");
        }

        order.ShippingName = address?.FullName ?? model.SelectedAddress?.FullName;
        order.ShippingPhone = address?.Phone ?? model.SelectedAddress?.Phone;
        order.ShippingAddress = address?.AddressLine ?? model.SelectedAddress?.AddressLine;
        order.ShippingWard = address?.Ward ?? model.SelectedAddress?.Ward;
        order.ShippingDistrict = address?.District ?? model.SelectedAddress?.District;
        order.ShippingCity = address?.City ?? model.SelectedAddress?.City;
        order.PaymentMethod = model.SelectedPaymentMethod;
        order.Notes = model.OrderNotes;
        await _orderService.UpdateOrderAsync(order);

        if (model.SelectedPaymentMethod == PaymentMethod.COD)
        {
            try
            {
                await _orderService.UpdateOrderStatusAsync(order.Id, OrderStatus.Confirmed);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ToastError"] = ex.Message;
                return RedirectToAction("Index", "Cart");
            }
            await _cartService.RemoveItemsAsync(userId.Value, cartItems.Select(i => i.Id));
            HttpContext.Session.Remove(CartController.CheckoutItemIdsSessionKey);
            TempData["ToastSuccess"] = "Order placed successfully!";
            return RedirectToAction("Details", "Order", new { id = order.Id });
        }

        return RedirectToAction("Index", "Payment", new { orderId = order.Id, method = model.SelectedPaymentMethod });
    }

    private List<CartItem> GetSelectedCartItems(List<CartItem> cartItems)
    {
        var selectedIds = HttpContext.Session.GetString(CartController.CheckoutItemIdsSessionKey);
        if (string.IsNullOrWhiteSpace(selectedIds))
            return cartItems;

        var ids = selectedIds
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(id => int.TryParse(id, out var value) ? value : 0)
            .Where(id => id > 0)
            .ToHashSet();

        return cartItems.Where(item => ids.Contains(item.Id)).ToList();
    }

    private static bool IsAtLeast16(DateTime birthday)
    {
        var today = DateTime.UtcNow.Date;
        return birthday.Date <= today.AddYears(-16);
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

        var coupon = await _couponRepository.GetByCodeAsync(couponCode);
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

        TempData["ToastSuccess"] = $"Coupon applied: {coupon.Code}";
        return RedirectToAction(nameof(Index), new { coupon = couponCode });
    }
}
