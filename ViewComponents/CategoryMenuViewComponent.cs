using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebActionResults.Models;

namespace WebActionResults.ViewComponents;

public class CategoryMenuViewComponent : ViewComponent
{
    private readonly ShopDbContext _context;

    public CategoryMenuViewComponent(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(int? selectedCategoryId = null)
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.CategoryName)
            .Select(c => new CategoryMenuItemViewModel
            {
                CategoryId = c.Id,
                CategoryName = c.CategoryName,
                ProductCount = c.Products.Count,
                IsSelected = selectedCategoryId.HasValue && selectedCategoryId.Value == c.Id
            })
            .ToListAsync();

        return View(categories);
    }
}

public sealed class CategoryMenuItemViewModel
{
    public int CategoryId { get; init; }

    public string CategoryName { get; init; } = string.Empty;

    public int ProductCount { get; init; }

    public bool IsSelected { get; init; }
}
