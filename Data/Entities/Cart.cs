using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebActionResults.Data.Entities;

public class Cart
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}

public class CartItem
{
    public int Id { get; set; }

    public int CartId { get; set; }

    public int ProductId { get; set; }

    public int? VariantId { get; set; }

    [MaxLength(150)]
    public string ProductName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? VariantName { get; set; }

    [MaxLength(255)]
    public string? ImageUrl { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal BasePrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PriceAdjustment { get; set; }

    [MaxLength(255)]
    public string? PriceBreakdown { get; set; }

    public int Quantity { get; set; }

    [ForeignKey(nameof(CartId))]
    public virtual Cart Cart { get; set; } = null!;

    [NotMapped]
    public decimal Total => UnitPrice * Quantity;
}