using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebActionResults.Models;

namespace WebActionResults.Controllers;

public class CategoryController : Controller
{
    private readonly ShopDbContext _context;

    public CategoryController(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .Include(c => c.Products)
            .OrderBy(c => c.CategoryName)
            .ToListAsync();

        return View(categories);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.Categories
            .AsNoTracking()
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.CategoryID == id);

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CategoryID,CategoryName,Description")] Category category)
    {
        if (!ModelState.IsValid)
        {
            return View(category);
        }

        _context.Add(category);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Da them danh muc thanh cong.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("CategoryID,CategoryName,Description")] Category category)
    {
        if (id != category.CategoryID)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(category);
        }

        try
        {
            _context.Update(category);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Da cap nhat danh muc thanh cong.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await CategoryExistsAsync(category.CategoryID))
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.Categories
            .AsNoTracking()
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.CategoryID == id);

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.CategoryID == id);

        if (category == null)
        {
            TempData["ErrorMessage"] = "Khong tim thay danh muc can xoa.";
            return RedirectToAction(nameof(Index));
        }

        if (category.Products.Any())
        {
            TempData["ErrorMessage"] = "Khong the xoa danh muc dang duoc gan cho san pham.";
            return RedirectToAction(nameof(Index));
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Da xoa danh muc thanh cong.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> CategoryExistsAsync(int id)
    {
        return await _context.Categories.AnyAsync(e => e.CategoryID == id);
    }
}
