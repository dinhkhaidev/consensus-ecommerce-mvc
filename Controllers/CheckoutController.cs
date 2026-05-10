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

    public CheckoutController(
        ICartService cartService,
        IOrderService orderService,
        IUserService userService,
        ICouponRepository couponRepository)
    {
        _cartService = cartService;
        _orderService = orderService;
        _userService = userService;
        _couponRepository = couponRepository;
    }

    public async Task<IActionResult> Index()
    {
        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var cartItems = await _cartService.GetCartAsync(userId.Value);
        if (!cartItems.Any())
            return RedirectToAction("Index", "Cart");

        var addresses = await _userService.GetAddressesAsync(userId.Value);
        var defaultAddress = addresses.FirstOrDefault(a => a.IsDefault) ?? addresses.FirstOrDefault();
        var subtotal = cartItems.Sum(c => c.UnitPrice * c.Quantity);

        // Get coupon from session (applied in cart)
        string? appliedCouponCode = HttpContext.Session.GetString("COUPON_CODE");
        decimal discountAmount = 0;
        decimal shippingFee = 50000;

        if (!string.IsNullOrEmpty(appliedCouponCode))
        {
            var coupon = await _couponRepository.GetByCodeAsync(appliedCouponCode);
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
                ProductId = c.ProductId,
                VariantId = c.VariantId,
                ProductName = c.ProductName,
                VariantName = c.VariantName,
                ImageUrl = c.ImageUrl,
                UnitPrice = c.UnitPrice,
                Quantity = c.Quantity
            }).ToList(),
            SubTotal = subtotal,
            ShippingFee = shippingFee,
            DiscountAmount = discountAmount,
            CouponCode = appliedCouponCode,
            Total = subtotal - discountAmount + shippingFee,
            SelectedAddressId = defaultAddress?.Id,
            SelectedAddress = defaultAddress != null ? new AddressViewModel
            {
                Id = defaultAddress.Id,
                FullName = defaultAddress.FullName,
                Phone = defaultAddress.Phone,
                AddressLine = defaultAddress.AddressLine,
                Ward = defaultAddress.Ward,
                District = defaultAddress.District,
                City = defaultAddress.City
            } : null
        };

        ViewBag.Addresses = addresses;

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessCheckout(CheckoutViewModel model)
    {
        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var cartItems = await _cartService.GetCartAsync(userId.Value);
        if (!cartItems.Any())
            return RedirectToAction("Index", "Cart");

        Address? address = null;
        if (model.SelectedAddressId.HasValue)
        {
            var addresses = await _userService.GetAddressesAsync(userId.Value);
            address = addresses.FirstOrDefault(a => a.Id == model.SelectedAddressId);
        }

        // Validate: must have either selected address OR new address with required fields
        if (address == null && model.SelectedAddress == null)
        {
            ModelState.AddModelError("", "Please select or enter a shipping address.");
        }
        else if (address == null && model.SelectedAddress != null)
        {
            // Validate new address fields
            if (string.IsNullOrWhiteSpace(model.SelectedAddress.FullName) ||
                string.IsNullOrWhiteSpace(model.SelectedAddress.Phone) ||
                string.IsNullOrWhiteSpace(model.SelectedAddress.AddressLine) ||
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

        var order = await _orderService.CreateOrderFromCartAsync(
            userId.Value,
            model.SelectedAddressId,
            cartItems,
            model.CouponCode);

        order.ShippingName = address?.FullName ?? model.SelectedAddress?.FullName;
        order.ShippingPhone = address?.Phone ?? model.SelectedAddress?.Phone;
        order.ShippingAddress = address?.AddressLine ?? model.SelectedAddress?.AddressLine;
        order.ShippingWard = address?.Ward ?? model.SelectedAddress?.Ward;
        order.ShippingDistrict = address?.District ?? model.SelectedAddress?.District;
        order.ShippingCity = address?.City ?? model.SelectedAddress?.City;
        order.PaymentMethod = model.SelectedPaymentMethod;
        order.Notes = model.OrderNotes;

        if (model.SelectedPaymentMethod == PaymentMethod.COD)
        {
            await _orderService.UpdateOrderStatusAsync(order.Id, OrderStatus.Confirmed);
            await _cartService.ClearCartAsync(userId.Value);
            TempData["ToastSuccess"] = "Order placed successfully!";
            return RedirectToAction("Details", "Order", new { id = order.Id });
        }

        return RedirectToAction("Index", "Payment", new { orderId = order.Id, method = model.SelectedPaymentMethod });
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