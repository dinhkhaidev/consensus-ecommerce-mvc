using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WebActionResults.Data.Services;
using WebActionResults.Data.Entities;
using WebActionResults.ViewModels;
using WebActionResults.Models;

namespace WebActionResults.Controllers;

public class ProductController : Controller
{
    private readonly ICatalogService _catalogService;
    private readonly IUserService _userService;
    private readonly IWebSettingsService _settingsService;

    public ProductController(ICatalogService catalogService, IUserService userService, IWebSettingsService settingsService)
    {
        _catalogService = catalogService;
        _userService = userService;
        _settingsService = settingsService;
    }

    public async Task<IActionResult> Index(int? categoryId, string? search, int? minPrice, int? maxPrice, string? priceRange, string? sort, int page = 1, int pageSize = 12)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 12;

        // Parse price range presets into min/max values
        int? filterMinPrice = minPrice;
        int? filterMaxPrice = maxPrice;

        if (!string.IsNullOrEmpty(priceRange))
        {
            switch (priceRange)
            {
                case "under-2000000":
                    filterMaxPrice = 2000000;
                    break;
                case "2000000-5000000":
                    filterMinPrice = 2000000;
                    filterMaxPrice = 5000000;
                    break;
                case "5000000-10000000":
                    filterMinPrice = 5000000;
                    filterMaxPrice = 10000000;
                    break;
                case "above-10000000":
                    filterMinPrice = 10000000;
                    break;
            }
        }

        List<Product> products;
        int totalCount = 0;

        if (!string.IsNullOrWhiteSpace(search))
        {
            (products, totalCount) = await _catalogService.SearchProductsPaginatedAsync(search, page, pageSize, sort);
            ViewData["SearchTerm"] = search;
        }
        else if (categoryId.HasValue)
        {
            if (filterMinPrice.HasValue || filterMaxPrice.HasValue)
            {
                (products, totalCount) = await _catalogService.GetProductsByCategoryAndPriceRangeAsync(categoryId.Value, filterMinPrice, filterMaxPrice, page, pageSize, sort);
            }
            else
            {
                (products, totalCount) = await _catalogService.GetProductsByCategoryPaginatedAsync(categoryId.Value, page, pageSize, sort);
            }
            var category = await _catalogService.GetCategoryByIdAsync(categoryId.Value);
            ViewData["SelectedCategoryName"] = category?.CategoryName;
            ViewData["SelectedCategoryId"] = categoryId.Value;
        }
        else if (filterMinPrice.HasValue || filterMaxPrice.HasValue)
        {
            (products, totalCount) = await _catalogService.GetProductsByPriceRangeAsync(filterMinPrice, filterMaxPrice, page, pageSize, sort);
        }
        else
        {
            (products, totalCount) = await _catalogService.GetProductsPaginatedAsync(page, pageSize, sort);
        }

        var viewModels = products.Select(p => new ProductListViewModel
        {
            ProductId = p.Id,
            ProductName = p.ProductName,
            UnitPrice = p.UnitPrice,
            CategoryName = p.Category?.CategoryName,
            MainImageUrl = p.Images?.FirstOrDefault(i => i.IsMain)?.ImageUrl,
            HasVariants = p.Variants?.Any() ?? false,
            MinPrice = p.Variants?.Any() == true ? p.Variants.Min(v => p.UnitPrice + (v.PriceAdjustment ?? 0)) : null
        }).ToList();

        var categories = await _catalogService.GetAllCategoriesAsync();
        ViewBag.Categories = categories;

        ViewData["CurrentPage"] = page;
        ViewData["PageSize"] = pageSize;
        ViewData["TotalCount"] = totalCount;
        ViewData["TotalPages"] = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewData["SortBy"] = sort;

        return View(viewModels);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _catalogService.GetProductWithDetailsAsync(id);
        if (product == null)
            return NotFound();

        var reviews = await _catalogService.GetProductReviewsAsync(id);

        bool isInWishlist = false;
        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId.HasValue)
        {
            var wishlistService = HttpContext.RequestServices.GetService<IWishlistService>();
            if (wishlistService != null)
                isInWishlist = await wishlistService.IsInWishlistAsync(userId.Value, id);
        }

        var viewModel = new ProductDetailViewModel
        {
            ProductId = product.Id,
            ProductName = product.ProductName,
            Description = product.QuantityPerUnit,
            UnitPrice = product.UnitPrice,
            CategoryName = product.Category?.CategoryName,
            SupplierName = null,
            Variants = product.Variants?.Select(v => new ProductVariantViewModel
            {
                Id = v.Id,
                Size = v.Size,
                Color = v.Color,
                SKU = v.SKU,
                PriceAdjustment = v.PriceAdjustment ?? 0,
                StockQuantity = v.StockQuantity,
                Images = v.Images?.Select(i => new ProductImageViewModel
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl ?? "",
                    AltText = i.AltText,
                    IsMain = i.IsMain
                }).ToList() ?? new List<ProductImageViewModel>()
            }).ToList() ?? new List<ProductVariantViewModel>(),
            Images = product.Images?.Select(i => new ProductImageViewModel
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl ?? "",
                AltText = i.AltText,
                IsMain = i.IsMain
            }).ToList() ?? new List<ProductImageViewModel>(),
            Reviews = reviews.Select(r => new ReviewViewModel
            {
                Id = r.Id,
                UserName = r.User?.UserName ?? "Anonymous",
                UserAvatar = null,
                Comment = r.Comment,
                Rating = r.Rating,
                CreatedAt = r.CreatedAt
            }).ToList(),
            AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0,
            ReviewCount = reviews.Count,
            IsInWishlist = isInWishlist
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> DetailsPartial(int id)
    {
        var product = await _catalogService.GetProductWithDetailsAsync(id);
        if (product == null)
            return NotFound();

        var reviews = await _catalogService.GetProductReviewsAsync(id);

        var viewModel = new ProductDetailViewModel
        {
            ProductId = product.Id,
            ProductName = product.ProductName,
            Description = product.QuantityPerUnit,
            UnitPrice = product.UnitPrice,
            CategoryName = product.Category?.CategoryName,
            SupplierName = null,
            Variants = product.Variants?.Select(v => new ProductVariantViewModel
            {
                Id = v.Id,
                Size = v.Size,
                Color = v.Color,
                SKU = v.SKU,
                PriceAdjustment = v.PriceAdjustment ?? 0,
                StockQuantity = v.StockQuantity,
                Images = v.Images?.Select(i => new ProductImageViewModel
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl ?? "",
                    AltText = i.AltText,
                    IsMain = i.IsMain
                }).ToList() ?? new List<ProductImageViewModel>()
            }).ToList() ?? new List<ProductVariantViewModel>(),
            Images = product.Images?.Select(i => new ProductImageViewModel
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl ?? "",
                AltText = i.AltText,
                IsMain = i.IsMain
            }).ToList() ?? new List<ProductImageViewModel>(),
            Reviews = reviews.Select(r => new ReviewViewModel
            {
                Id = r.Id,
                UserName = r.User?.UserName ?? "Anonymous",
                UserAvatar = null,
                Comment = r.Comment,
                Rating = r.Rating,
                CreatedAt = r.CreatedAt
            }).ToList(),
            AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0,
            ReviewCount = reviews.Count,
            IsInWishlist = false,
            RelatedProducts = new List<ProductListViewModel>()
        };

        return PartialView("_DetailsPartial", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Compare(int id)
    {
        var product = await _catalogService.GetProductWithDetailsAsync(id);
        if (product == null)
            return NotFound();

        var viewModel = new ProductDetailViewModel
        {
            ProductId = product.Id,
            ProductName = product.ProductName,
            Description = product.QuantityPerUnit,
            UnitPrice = product.UnitPrice,
            CategoryName = product.Category?.CategoryName,
            SupplierName = null,
            Variants = product.Variants?.Select(v => new ProductVariantViewModel
            {
                Id = v.Id,
                Size = v.Size,
                Color = v.Color,
                SKU = v.SKU,
                PriceAdjustment = v.PriceAdjustment ?? 0,
                StockQuantity = v.StockQuantity,
                Images = v.Images?.Select(i => new ProductImageViewModel
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl ?? "",
                    AltText = i.AltText,
                    IsMain = i.IsMain
                }).ToList() ?? new List<ProductImageViewModel>()
            }).ToList() ?? new List<ProductVariantViewModel>(),
            Images = product.Images?.Select(i => new ProductImageViewModel
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl ?? "",
                AltText = i.AltText,
                IsMain = i.IsMain
            }).ToList() ?? new List<ProductImageViewModel>(),
            Reviews = new List<ReviewViewModel>(),
            AverageRating = 0,
            ReviewCount = 0,
            IsInWishlist = false,
            RelatedProducts = new List<ProductListViewModel>()
        };

        return View("Details", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(AddReviewViewModel model)
    {
        // Check if reviews are enabled
        var settings = await _settingsService.GetAllSettingsAsync();
        var reviewsEnabled = string.Equals(settings.GetValueOrDefault("EnableReviews", "true"), "true", StringComparison.OrdinalIgnoreCase);
        if (!reviewsEnabled)
        {
            TempData["ToastError"] = "Reviews are currently disabled.";
            return RedirectToAction(nameof(Details), new { id = model.ProductId });
        }

        if (!ModelState.IsValid || model.Rating < 1 || model.Rating > 5)
        {
            return RedirectToAction(nameof(Details), new { id = model.ProductId });
        }

        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var autoApprove = string.Equals(settings.GetValueOrDefault("AutoApproveReviews", "false"), "true", StringComparison.OrdinalIgnoreCase);

        var review = new Review
        {
            ProductId = model.ProductId,
            UserId = userId.Value,
            Comment = model.Comment,
            Rating = model.Rating,
            IsApproved = autoApprove,
            CreatedAt = DateTime.UtcNow
        };

        await _catalogService.AddReviewAsync(review);
        TempData["ToastSuccess"] = autoApprove
            ? "Review submitted successfully!"
            : "Review submitted. It will be displayed after approval.";

        return RedirectToAction(nameof(Details), new { id = model.ProductId });
    }
}
