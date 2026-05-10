using WebActionResults.Data.Entities;

namespace WebActionResults.ViewModels;

public class WishlistViewModel
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string? MainImageUrl { get; set; }
    public DateTime AddedAt { get; set; }
}