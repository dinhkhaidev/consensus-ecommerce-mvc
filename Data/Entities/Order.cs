using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebActionResults.Models;

namespace WebActionResults.Data.Entities;

public class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required]
    [MaxLength(20)]
    public string OrderNumber { get; set; } = string.Empty;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ShippingFee { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public int? AddressId { get; set; }

    [MaxLength(100)]
    public string? ShippingName { get; set; }

    [MaxLength(20)]
    public string? ShippingPhone { get; set; }

    [MaxLength(255)]
    public string? ShippingAddress { get; set; }

    [MaxLength(100)]
    public string? ShippingCity { get; set; }

    [MaxLength(100)]
    public string? ShippingDistrict { get; set; }

    [MaxLength(100)]
    public string? ShippingWard { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public PaymentMethod? PaymentMethod { get; set; }

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? ShippedAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public int? CouponId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual Account User { get; set; } = null!;

    [ForeignKey(nameof(AddressId))]
    public virtual Address? Address { get; set; }

    [ForeignKey(nameof(CouponId))]
    public virtual Coupon? Coupon { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5
}

public enum PaymentMethod
{
    COD = 0,
    VNPay = 1,
    MoMo = 2,
    BankTransfer = 3
}

public enum PaymentStatus
{
    Pending = 0,
    Paid = 1,
    Failed = 2,
    Refunded = 3
}
