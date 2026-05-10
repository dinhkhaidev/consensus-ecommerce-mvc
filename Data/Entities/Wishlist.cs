using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebActionResults.Models;

namespace WebActionResults.Data.Entities;

public class Wishlist
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ProductId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public virtual Account User { get; set; } = null!;

    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; } = null!;
}