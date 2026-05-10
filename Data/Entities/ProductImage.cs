using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebActionResults.Models;

namespace WebActionResults.Data.Entities;

public class ProductImage
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int? VariantId { get; set; }

    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? AltText { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsMain { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey(nameof(VariantId))]
    public virtual ProductVariant? Variant { get; set; }
}