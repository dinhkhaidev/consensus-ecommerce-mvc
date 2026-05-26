using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebActionResults.Models;

namespace WebActionResults.Areas.Admin.Controllers;

[Area("Admin")]
public class AdminCategoryController : AdminControllerBase
{
    private readonly ShopDbContext _context;

    public AdminCategoryController(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories
            .Include(c => c.Products)
            .OrderBy(c => c.CategoryName)
            .ToListAsync();

        return View(categories);
    }

    public async Task<IActionResult> Details(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products).ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return NotFound();
        return View(category);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            TempData["ToastSuccess"] = "Category created successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();
        return View(category);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            TempData["ToastSuccess"] = "Category updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (category != null)
        {
            if (category.Products.Any())
            {
                TempData["ToastWarning"] = "Category still has products. Move or edit those products before deleting this category.";
                return RedirectToAction(nameof(Index));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            TempData["ToastSuccess"] = "Category deleted successfully!";
        }
        return RedirectToAction(nameof(Index));
    }
}
