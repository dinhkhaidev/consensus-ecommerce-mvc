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
    Task UpdateOrderStatusAsync(int orderId, OrderStatus status);
    Task UpdatePaymentStatusAsync(int orderId, PaymentStatus status);
    Task<string> GetOrderStatusText(OrderStatus status);
    Task<string> GetPaymentStatusText(PaymentStatus status);
}

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly IProductRepository _productRepository;

    public OrderService(IOrderRepository orderRepository, ICouponRepository couponRepository, IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _couponRepository = couponRepository;
        _productRepository = productRepository;
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId)
        => await _orderRepository.GetByIdAsync(orderId);

    public async Task<Order?> GetOrderByOrderNumberAsync(string orderNumber)
        => await _orderRepository.GetByOrderNumberAsync(orderNumber);

    public async Task<List<Order>> GetUserOrdersAsync(int userId)
        => await _orderRepository.GetByUserIdAsync(userId);

    public async Task<Order> CreateOrderFromCartAsync(int userId, int? addressId, List<WebActionResults.Data.Entities.CartItem> cartItems, string? couponCode = null)
    {
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

    public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return;

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;

        if (status == OrderStatus.Shipped)
            order.ShippedAt = DateTime.UtcNow;
        else if (status == OrderStatus.Delivered)
            order.DeliveredAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order);
    }

    public async Task UpdatePaymentStatusAsync(int orderId, PaymentStatus status)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return;

        order.PaymentStatus = status;
        order.UpdatedAt = DateTime.UtcNow;

        if (status == PaymentStatus.Paid)
            order.Status = OrderStatus.Confirmed;

        await _orderRepository.UpdateAsync(order);
    }

    public Task<string> GetOrderStatusText(OrderStatus status) => Task.FromResult(status switch
    {
        OrderStatus.Pending => "Pending",
        OrderStatus.Confirmed => "Confirmed",
        OrderStatus.Processing => "Processing",
        OrderStatus.Shipped => "Shipped",
        OrderStatus.Delivered => "Delivered",
        OrderStatus.Cancelled => "Cancelled",
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
}