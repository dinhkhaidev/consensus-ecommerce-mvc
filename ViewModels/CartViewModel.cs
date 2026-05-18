using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using WebActionResults.Data.Entities;

namespace WebActionResults.ViewModels;

public class CartViewModel
{
    public List<CartItemViewModel> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public string? AppliedCouponCode { get; set; }
    public string? CouponError { get; set; }
}

public class CartItemViewModel
{
    public int CartItemId { get; set; }
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal PriceAdjustment { get; set; }
    public decimal BasePrice { get; set; }
    public string? PriceBreakdown { get; set; }
    public int Quantity { get; set; }
    public decimal Total => UnitPrice * Quantity;
}

public class CheckoutViewModel
{
    public List<CartItemViewModel> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public int? SelectedAddressId { get; set; }
    public AddressViewModel? SelectedAddress { get; set; }
    public string AddressEntryMode { get; set; } = "saved";
    public bool SaveNewAddressAsDefault { get; set; }
    public string? CouponCode { get; set; }
    public PaymentMethod SelectedPaymentMethod { get; set; } = PaymentMethod.COD;
    public string? OrderNotes { get; set; }
}

public class OrderViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal TotalAmount { get; set; }
    public string? ShippingAddress { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string PaymentStatusText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? CancelReason { get; set; }
    public DateTime? CancelRequestedAt { get; set; }
    public bool? CancelApproved { get; set; }
    public string? CancelAdminNote { get; set; }
    public DateTime? CancelReviewedAt { get; set; }
    public string? ReturnReason { get; set; }
    public string? ReturnImageUrl { get; set; }
    public DateTime? ReturnRequestedAt { get; set; }
    public bool? ReturnApproved { get; set; }
    public string? ReturnAdminNote { get; set; }
    public DateTime? ReturnReviewedAt { get; set; }
    public bool CanRequestCancellation { get; set; }
    public bool CanRequestReturn { get; set; }
    public List<OrderItemViewModel> Items { get; set; } = new();
}

public class OrderItemViewModel
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public string? ImageUrl { get; set; }
}

public class ApplyCouponViewModel
{
    [Required]
    [StringLength(50)]
    public string CouponCode { get; set; } = string.Empty;
}

public class CancelOrderRequestViewModel
{
    public int OrderId { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 5)]
    public string Reason { get; set; } = string.Empty;
}

public class ReturnOrderRequestViewModel
{
    public int OrderId { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Reason { get; set; } = string.Empty;

    [Required]
    public IFormFile? Image { get; set; }
}

public class OrderListViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public int ItemCount { get; set; }
}
