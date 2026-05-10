using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebActionResults.Data.Entities;

public class Coupon
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    public CouponType Type { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountValue { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinOrderAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaxDiscountAmount { get; set; }

    public int UsageLimit { get; set; }

    public int UsedCount { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}

public enum CouponType
{
    Percentage = 0,
    FixedAmount = 1,
    FreeShipping = 2
}