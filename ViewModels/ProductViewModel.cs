using WebActionResults.Data.Entities;

namespace WebActionResults.ViewModels;

public class ProductListViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string? CategoryName { get; set; }
    public string? MainImageUrl { get; set; }
    public bool HasVariants { get; set; }
    public decimal? MinPrice { get; set; }
}

public class ProductDetailViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? OriginalPrice { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? SupplierName { get; set; }
    public List<ProductVariantViewModel> Variants { get; set; } = new();
    public List<ProductImageViewModel> Images { get; set; } = new();
    public List<ReviewViewModel> Reviews { get; set; } = new();
    public List<ProductListViewModel> RelatedProducts { get; set; } = new();
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsInWishlist { get; set; }
    public string? Room3DProductId { get; set; }
    public bool HasRoom3DModel => !string.IsNullOrWhiteSpace(Room3DProductId);
}

public class ProductVariantViewModel
{
    public int Id { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public string? SKU { get; set; }
    public decimal? PriceAdjustment { get; set; }
    public int StockQuantity { get; set; }
    public List<ProductImageViewModel> Images { get; set; } = new();
}

public class ProductImageViewModel
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsMain { get; set; }
}

public class ReviewViewModel
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public string Comment { get; set; } = string.Empty;
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CategoryViewModel
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ProductCount { get; set; }
    public List<CategoryViewModel> SubCategories { get; set; } = new();
}

public class AddReviewViewModel
{
    public int ProductId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}
