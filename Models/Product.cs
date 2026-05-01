using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebActionResults.Models;

public partial class Product
{
    [Key]
    public int ProductID { get; set; }

    [StringLength(150)]
    public string ProductName { get; set; } = null!;

    public int SupplierID { get; set; }

    public int CategoryID { get; set; }

    [StringLength(100)]
    public string? QuantityPerUnit { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal UnitPrice { get; set; }

    public bool Discontinued { get; set; }

    [ForeignKey("CategoryID")]
    [InverseProperty("Products")]
    public virtual Category Category { get; set; } = null!;

    [ForeignKey("SupplierID")]
    [InverseProperty("Products")]
    public virtual Supplier Supplier { get; set; } = null!;
}
