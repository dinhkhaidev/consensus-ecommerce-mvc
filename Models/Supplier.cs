using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebActionResults.Models;

public partial class Supplier
{
    [Key]
    public int SupplierID { get; set; }

    [StringLength(150)]
    public string CompanyName { get; set; } = null!;

    [StringLength(30)]
    [Unicode(false)]
    public string? Phone { get; set; }

    [InverseProperty("Supplier")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
