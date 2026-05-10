using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebActionResults.Data.Entities;
using WebActionResults.Data.Repositories;
using WebActionResults.Data.Services;
using WebActionResults.ViewModels;

namespace WebActionResults.Controllers;

[Authorize]
public class OrderController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;
    private readonly IUserService _userService;
    private readonly ICouponRepository _couponRepository;

    public OrderController(
        IOrderService orderService,
        ICartService cartService,
        IUserService userService,
        ICouponRepository couponRepository)
    {
        _orderService = orderService;
        _cartService = cartService;
        _userService = userService;
        _couponRepository = couponRepository;
    }

    public async Task<IActionResult> Index()
    {
        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var orders = await _orderService.GetUserOrdersAsync(userId.Value);
        var viewModels = orders.Select(o => new OrderListViewModel
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            CreatedAt = o.CreatedAt,
            TotalAmount = o.TotalAmount,
            Status = o.Status,
            StatusText = GetStatusText(o.Status),
            ItemCount = o.OrderItems?.Count ?? 0
        }).ToList();

        return View(viewModels);
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null || order.UserId != userId.Value)
            return NotFound();

        var viewModel = new OrderViewModel
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            StatusText = GetStatusText(order.Status),
            SubTotal = order.SubTotal,
            DiscountAmount = order.DiscountAmount,
            ShippingFee = order.ShippingFee,
            TotalAmount = order.TotalAmount,
            ShippingAddress = FormatAddress(order),
            PaymentMethod = order.PaymentMethod ?? PaymentMethod.COD,
            PaymentStatus = order.PaymentStatus,
            PaymentStatusText = GetPaymentStatusText(order.PaymentStatus),
            CreatedAt = order.CreatedAt,
            Items = order.OrderItems?.Select(i => new OrderItemViewModel
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                VariantName = i.VariantName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                TotalPrice = i.TotalPrice
            }).ToList() ?? new()
        };

        return View(viewModel);
    }

    private string FormatAddress(Order order)
    {
        var parts = new[] { order.ShippingAddress, order.ShippingWard, order.ShippingDistrict, order.ShippingCity }
            .Where(p => !string.IsNullOrWhiteSpace(p));
        return string.Join(", ", parts);
    }

    private string GetStatusText(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "Pending",
        OrderStatus.Confirmed => "Confirmed",
        OrderStatus.Processing => "Processing",
        OrderStatus.Shipped => "Shipped",
        OrderStatus.Delivered => "Delivered",
        OrderStatus.Cancelled => "Cancelled",
        _ => "Unknown"
    };

    private string GetPaymentStatusText(PaymentStatus status) => status switch
    {
        PaymentStatus.Pending => "Pending",
        PaymentStatus.Paid => "Paid",
        PaymentStatus.Failed => "Failed",
        PaymentStatus.Refunded => "Refunded",
        _ => "Unknown"
    };
}