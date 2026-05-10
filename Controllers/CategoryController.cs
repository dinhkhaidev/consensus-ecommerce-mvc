using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WebActionResults.Data.Services;
using WebActionResults.ViewModels;
using WebActionResults.Models;

namespace WebActionResults.Controllers;

public class CategoryController : Controller
{
    private readonly ICatalogService _catalogService;

    public CategoryController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _catalogService.GetCategoriesWithProductsAsync();
        var viewModels = categories.Select(c => new CategoryViewModel
        {
            CategoryId = c.Id,
            CategoryName = c.CategoryName,
            Description = null, // Description column doesn't exist in dtb.sql
            ProductCount = c.Products?.Count ?? 0
        }).ToList();

        return View(viewModels);
    }

    public async Task<IActionResult> Details(int id, int page = 1, int pageSize = 12)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 12;

        var category = await _catalogService.GetCategoryByIdAsync(id);
        if (category == null)
            return NotFound();

        var (products, totalCount) = await _catalogService.GetProductsByCategoryPaginatedAsync(id, page, pageSize);

        var viewModel = new CategoryViewModel
        {
            CategoryId = category.Id,
            CategoryName = category.CategoryName,
            Description = null, // Description column doesn't exist in dtb.sql
            ProductCount = totalCount
        };

        ViewBag.Products = products.Select(p => new ProductListViewModel
        {
            ProductId = p.Id,
            ProductName = p.ProductName,
            UnitPrice = p.UnitPrice,
            CategoryName = p.Category?.CategoryName,
            MainImageUrl = p.Images?.FirstOrDefault(i => i.IsMain)?.ImageUrl,
            HasVariants = p.Variants?.Any() ?? false
        }).ToList();

        ViewData["CurrentPage"] = page;
        ViewData["PageSize"] = pageSize;
        ViewData["TotalCount"] = totalCount;
        ViewData["TotalPages"] = (int)Math.Ceiling(totalCount / (double)pageSize);

        return View(viewModel);
    }
}