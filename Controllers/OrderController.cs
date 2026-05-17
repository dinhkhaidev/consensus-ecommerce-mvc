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
    private readonly IWebHostEnvironment _environment;

    public OrderController(
        IOrderService orderService,
        ICartService cartService,
        IUserService userService,
        ICouponRepository couponRepository,
        IWebHostEnvironment environment)
    {
        _orderService = orderService;
        _cartService = cartService;
        _userService = userService;
        _couponRepository = couponRepository;
        _environment = environment;
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
            CancelReason = order.CancelReason,
            CancelRequestedAt = order.CancelRequestedAt,
            CancelApproved = order.CancelApproved,
            CancelAdminNote = order.CancelAdminNote,
            CancelReviewedAt = order.CancelReviewedAt,
            ReturnReason = order.ReturnReason,
            ReturnImageUrl = order.ReturnImageUrl,
            ReturnRequestedAt = order.ReturnRequestedAt,
            ReturnApproved = order.ReturnApproved,
            ReturnAdminNote = order.ReturnAdminNote,
            ReturnReviewedAt = order.ReturnReviewedAt,
            CanRequestCancellation = order.Status is OrderStatus.Pending or OrderStatus.Confirmed or OrderStatus.Processing,
            CanRequestReturn = order.Status == OrderStatus.Delivered,
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestCancellation(CancelOrderRequestViewModel model)
    {
        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid)
        {
            TempData["ToastError"] = "Please enter a cancellation reason.";
            return RedirectToAction(nameof(Details), new { id = model.OrderId });
        }

        try
        {
            await _orderService.RequestCancellationAsync(model.OrderId, userId.Value, model.Reason);
            TempData["ToastSuccess"] = "Cancellation request sent. Admin will review it soon.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ToastError"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = model.OrderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestReturn(ReturnOrderRequestViewModel model)
    {
        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid || model.Image == null)
        {
            TempData["ToastError"] = "Please enter a return reason and upload an image.";
            return RedirectToAction(nameof(Details), new { id = model.OrderId });
        }

        var imageUrl = await SaveReturnImageAsync(model.Image);
        if (imageUrl == null)
        {
            TempData["ToastError"] = "Return image must be JPG, PNG, WEBP or GIF and no larger than 5MB.";
            return RedirectToAction(nameof(Details), new { id = model.OrderId });
        }

        try
        {
            await _orderService.RequestReturnAsync(model.OrderId, userId.Value, model.Reason, imageUrl);
            TempData["ToastSuccess"] = "Return request sent. Admin will review your reason and image.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ToastError"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = model.OrderId });
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
        OrderStatus.CancellationRequested => "Cancellation Requested",
        OrderStatus.ReturnRequested => "Return Requested",
        OrderStatus.ReturnApproved => "Return Approved",
        OrderStatus.ReturnRejected => "Return Rejected",
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

    private async Task<string?> SaveReturnImageAsync(IFormFile image)
    {
        const long maxSize = 5 * 1024 * 1024;
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp", ".gif"
        };

        if (image.Length <= 0 || image.Length > maxSize)
            return null;

        var extension = Path.GetExtension(image.FileName);
        if (!allowedExtensions.Contains(extension))
            return null;

        var folder = Path.Combine(_environment.WebRootPath, "uploads", "returns");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var path = Path.Combine(folder, fileName);

        await using var stream = System.IO.File.Create(path);
        await image.CopyToAsync(stream);

        return $"/uploads/returns/{fileName}";
    }
}
