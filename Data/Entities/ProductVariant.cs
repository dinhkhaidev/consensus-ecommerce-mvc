using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebActionResults.Models;

namespace WebActionResults.Data.Entities;

public class ProductVariant
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    [MaxLength(50)]
    public string? Size { get; set; }

    [MaxLength(50)]
    public string? Color { get; set; }

    [MaxLength(100)]
    public string? SKU { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PriceAdjustment { get; set; }

    public int StockQuantity { get; set; }

    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
}