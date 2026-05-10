using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebActionResults.Models;

public partial class Category
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string CategoryName { get; set; } = null!;

    [StringLength(255)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public int ProductCount => Products?.Count ?? 0;

    [InverseProperty("Category")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}