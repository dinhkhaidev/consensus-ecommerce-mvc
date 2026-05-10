using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using WebActionResults.Data.Entities;

namespace WebActionResults.Models;

public partial class Product
{
    [Key]
    public int Id { get; set; }

    [StringLength(150)]
    public string ProductName { get; set; } = null!;

    public int CategoryID { get; set; }

    public int? SupplierID { get; set; }

    [StringLength(100)]
    public string? QuantityPerUnit { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal UnitPrice { get; set; }

    public bool Discontinued { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("CategoryID")]
    [InverseProperty("Products")]
    public virtual Category Category { get; set; } = null!;

    [ForeignKey("SupplierID")]
    [InverseProperty("Products")]
    public virtual Supplier Supplier { get; set; } = null!;

    public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}