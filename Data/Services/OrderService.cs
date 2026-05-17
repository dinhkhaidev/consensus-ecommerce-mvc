using Microsoft.EntityFrameworkCore;
using WebActionResults.Data.Entities;
using WebActionResults.Data.Repositories;
using WebActionResults.Models;

namespace WebActionResults.Data.Services;

public interface IOrderService
{
    Task<Order?> GetOrderByIdAsync(int orderId);
    Task<Order?> GetOrderByOrderNumberAsync(string orderNumber);
    Task<List<Order>> GetUserOrdersAsync(int userId);
    Task<Order> CreateOrderFromCartAsync(int userId, int? addressId, List<WebActionResults.Data.Entities.CartItem> cartItems, string? couponCode = null);
    Task UpdateOrderAsync(Order order);
    Task UpdateOrderStatusAsync(int orderId, OrderStatus status);
    Task UpdatePaymentStatusAsync(int orderId, PaymentStatus status);
    Task RequestCancellationAsync(int orderId, int userId, string reason);
    Task RequestReturnAsync(int orderId, int userId, string reason, string imageUrl);
    Task ReviewCancellationAsync(int orderId, bool approve, string? adminNote);
    Task ReviewReturnAsync(int orderId, bool approve, string? adminNote);
    Task<string> GetOrderStatusText(OrderStatus status);
    Task<string> GetPaymentStatusText(PaymentStatus status);
}

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly IProductRepository _productRepository;
    private readonly ShopDbContext _context;

    public OrderService(
        IOrderRepository orderRepository,
        ICouponRepository couponRepository,
        IProductRepository productRepository,
        ShopDbContext context)
    {
        _orderRepository = orderRepository;
        _couponRepository = couponRepository;
        _productRepository = productRepository;
        _context = context;
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId)
        => await _orderRepository.GetByIdAsync(orderId);

    public async Task<Order?> GetOrderByOrderNumberAsync(string orderNumber)
        => await _orderRepository.GetByOrderNumberAsync(orderNumber);

    public async Task<List<Order>> GetUserOrdersAsync(int userId)
        => await _orderRepository.GetByUserIdAsync(userId);

    public async Task<Order> CreateOrderFromCartAsync(int userId, int? addressId, List<WebActionResults.Data.Entities.CartItem> cartItems, string? couponCode = null)
    {
        await EnsureStockAvailableAsync(cartItems);

        var orderNumber = await _orderRepository.GenerateOrderNumberAsync();

        decimal subtotal = cartItems.Sum(c => c.UnitPrice * c.Quantity);
        decimal discountAmount = 0;
        decimal shippingFee = 50000;
        Coupon? coupon = null;

        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            coupon = await _couponRepository.GetByCodeAsync(couponCode);
            if (coupon != null && ValidateCoupon(coupon, subtotal))
            {
                discountAmount = CalculateDiscount(coupon, subtotal);
                shippingFee = coupon.Type == CouponType.FreeShipping ? 0 : shippingFee;
            }
            else
            {
                coupon = null;
            }
        }

        var order = new Order
        {
            OrderNumber = orderNumber,
            UserId = userId,
            AddressId = addressId,
            SubTotal = subtotal,
            DiscountAmount = discountAmount,
            ShippingFee = shippingFee,
            TotalAmount = subtotal - discountAmount + shippingFee,
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            CouponId = coupon?.Id,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in cartItems)
        {
            order.OrderItems.Add(new OrderItem
            {
                ProductId = item.ProductId,
                VariantId = item.VariantId,
                ProductName = item.ProductName,
                VariantName = item.VariantName,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                TotalPrice = item.UnitPrice * item.Quantity
            });
        }

        return await _orderRepository.CreateAsync(order);
    }

    public async Task UpdateOrderAsync(Order order)
    {
        await _orderRepository.UpdateAsync(order);
    }

    public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return;

        var previousStatus = order.Status;
        var shouldFinalizeOrder = status == OrderStatus.Confirmed && previousStatus == OrderStatus.Pending;
        var shouldReverseOrder = status == OrderStatus.Cancelled && previousStatus != OrderStatus.Cancelled && HasDeductedStock(order, previousStatus);
        var shouldApproveReturn = status == OrderStatus.ReturnApproved && previousStatus == OrderStatus.ReturnRequested;

        await using var transaction = shouldFinalizeOrder || shouldReverseOrder || shouldApproveReturn
            ? await _context.Database.BeginTransactionAsync()
            : null;

        if (shouldFinalizeOrder)
        {
            await FinalizeOrderAsync(order);
        }
        else if (shouldReverseOrder || shouldApproveReturn)
        {
            await ReverseFinalizedOrderAsync(order);
            if (order.PaymentStatus == PaymentStatus.Paid)
                order.PaymentStatus = PaymentStatus.Refunded;
        }

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;

        if (status == OrderStatus.Shipped)
            order.ShippedAt = DateTime.UtcNow;
        else if (status == OrderStatus.Delivered)
            order.DeliveredAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order);
        if (transaction != null)
            await transaction.CommitAsync();
    }

    public async Task UpdatePaymentStatusAsync(int orderId, PaymentStatus status)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return;

        order.PaymentStatus = status;
        order.UpdatedAt = DateTime.UtcNow;

        if (status == PaymentStatus.Paid)
            order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order);
    }

    public async Task RequestCancellationAsync(int orderId, int userId, string reason)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null || order.UserId != userId)
            throw new InvalidOperationException("Order not found.");

        if (!CanRequestCancellation(order.Status))
            throw new InvalidOperationException("This order can no longer be cancelled.");

        order.CancelReason = reason.Trim();
        order.CancelRequestedAt = DateTime.UtcNow;
        order.CancelRequestedFromStatus = order.Status;
        order.CancelApproved = null;
        order.CancelAdminNote = null;
        order.CancelReviewedAt = null;
        order.Status = OrderStatus.CancellationRequested;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order);
    }

    public async Task RequestReturnAsync(int orderId, int userId, string reason, string imageUrl)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null || order.UserId != userId)
            throw new InvalidOperationException("Order not found.");

        if (order.Status != OrderStatus.Delivered)
            throw new InvalidOperationException("Only delivered orders can be returned.");

        order.ReturnReason = reason.Trim();
        order.ReturnImageUrl = imageUrl;
        order.ReturnRequestedAt = DateTime.UtcNow;
        order.ReturnRequestedFromStatus = order.Status;
        order.ReturnApproved = null;
        order.ReturnAdminNote = null;
        order.ReturnReviewedAt = null;
        order.Status = OrderStatus.ReturnRequested;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order);
    }

    public async Task ReviewCancellationAsync(int orderId, bool approve, string? adminNote)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new InvalidOperationException("Order not found.");

        if (order.Status != OrderStatus.CancellationRequested)
            throw new InvalidOperationException("This order is not waiting for cancellation review.");

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var originalStatus = order.CancelRequestedFromStatus ?? OrderStatus.Pending;
        order.CancelApproved = approve;
        order.CancelAdminNote = adminNote?.Trim();
        order.CancelReviewedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        if (approve)
        {
            if (HasDeductedStock(order, originalStatus))
                await ReverseFinalizedOrderAsync(order);

            if (order.PaymentStatus == PaymentStatus.Paid)
                order.PaymentStatus = PaymentStatus.Refunded;

            order.Status = OrderStatus.Cancelled;
        }
        else
        {
            order.Status = originalStatus;
        }

        await _orderRepository.UpdateAsync(order);
        await transaction.CommitAsync();
    }

    public async Task ReviewReturnAsync(int orderId, bool approve, string? adminNote)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new InvalidOperationException("Order not found.");

        if (order.Status != OrderStatus.ReturnRequested)
            throw new InvalidOperationException("This order is not waiting for return review.");

        await using var transaction = await _context.Database.BeginTransactionAsync();

        order.ReturnApproved = approve;
        order.ReturnAdminNote = adminNote?.Trim();
        order.ReturnReviewedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        if (approve)
        {
            await ReverseFinalizedOrderAsync(order);
            if (order.PaymentStatus == PaymentStatus.Paid)
                order.PaymentStatus = PaymentStatus.Refunded;

            order.Status = OrderStatus.ReturnApproved;
        }
        else
        {
            order.Status = OrderStatus.ReturnRejected;
        }

        await _orderRepository.UpdateAsync(order);
        await transaction.CommitAsync();
    }

    public Task<string> GetOrderStatusText(OrderStatus status) => Task.FromResult(status switch
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
    });

    public Task<string> GetPaymentStatusText(PaymentStatus status) => Task.FromResult(status switch
    {
        PaymentStatus.Pending => "Pending",
        PaymentStatus.Paid => "Paid",
        PaymentStatus.Failed => "Failed",
        PaymentStatus.Refunded => "Refunded",
        _ => "Unknown"
    });

    private bool ValidateCoupon(Coupon coupon, decimal orderSubtotal)
    {
        var now = DateTime.UtcNow;
        return coupon.IsActive &&
               coupon.StartDate <= now &&
               coupon.EndDate >= now &&
               coupon.UsedCount < coupon.UsageLimit &&
               (!coupon.MinOrderAmount.HasValue || orderSubtotal >= coupon.MinOrderAmount.Value);
    }

    private decimal CalculateDiscount(Coupon coupon, decimal subtotal)
    {
        decimal discount = coupon.Type switch
        {
            CouponType.Percentage => subtotal * (coupon.DiscountValue / 100),
            CouponType.FixedAmount => coupon.DiscountValue,
            CouponType.FreeShipping => 0,
            _ => 0
        };

        if (coupon.MaxDiscountAmount.HasValue && discount > coupon.MaxDiscountAmount.Value)
            discount = coupon.MaxDiscountAmount.Value;

        return discount;
    }

    private async Task EnsureStockAvailableAsync(IEnumerable<WebActionResults.Data.Entities.CartItem> cartItems)
    {
        foreach (var item in cartItems.Where(i => i.VariantId.HasValue))
        {
            var variantId = item.VariantId.GetValueOrDefault();
            var variants = await _productRepository.GetVariantsAsync(item.ProductId);
            var variant = variants.FirstOrDefault(v => v.Id == variantId);
            if (variant == null || variant.StockQuantity < item.Quantity)
                throw new InvalidOperationException($"{item.ProductName} ({item.VariantName}) does not have enough stock.");
        }
    }

    private async Task FinalizeOrderAsync(Order order)
    {
        await DeductStockForOrderAsync(order);

        if (order.CouponId.HasValue)
        {
            var couponIncremented = await _couponRepository.IncrementUsageAsync(order.CouponId.Value);
            if (!couponIncremented)
                throw new InvalidOperationException("This coupon has reached its usage limit or is no longer active.");
        }
    }

    private async Task DeductStockForOrderAsync(Order order)
    {
        foreach (var item in order.OrderItems.Where(i => i.VariantId.HasValue))
        {
            var variantId = item.VariantId.GetValueOrDefault();
            var deducted = await _productRepository.DeductStockAsync(variantId, item.Quantity);
            if (!deducted)
                throw new InvalidOperationException($"{item.ProductName} ({item.VariantName}) does not have enough stock.");
        }
    }

    private async Task ReverseFinalizedOrderAsync(Order order)
    {
        foreach (var item in order.OrderItems.Where(i => i.VariantId.HasValue))
        {
            var variantId = item.VariantId.GetValueOrDefault();
            var restored = await _productRepository.RestoreStockAsync(variantId, item.Quantity);
            if (!restored)
                throw new InvalidOperationException($"{item.ProductName} ({item.VariantName}) stock could not be restored.");
        }

        if (order.CouponId.HasValue)
            await _couponRepository.DecrementUsageAsync(order.CouponId.Value);
    }

    private static bool CanRequestCancellation(OrderStatus status)
        => status is OrderStatus.Pending or OrderStatus.Confirmed or OrderStatus.Processing;

    private static bool HasDeductedStock(Order order, OrderStatus status)
        => status switch
        {
            OrderStatus.Confirmed or OrderStatus.Processing or OrderStatus.Shipped or OrderStatus.Delivered or
                OrderStatus.ReturnRequested or OrderStatus.ReturnRejected or OrderStatus.ReturnApproved => true,
            OrderStatus.CancellationRequested => order.CancelRequestedFromStatus.HasValue &&
                                                 HasDeductedStock(order, order.CancelRequestedFromStatus.Value),
            _ => false
        };
}
