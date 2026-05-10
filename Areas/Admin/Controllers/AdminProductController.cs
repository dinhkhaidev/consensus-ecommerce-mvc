using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebActionResults.Models;

namespace WebActionResults.Areas.Admin.Controllers;

[Area("Admin")]
public class AdminProductController : AdminControllerBase
{
    private readonly ShopDbContext _context;

    public AdminProductController(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string search = "")
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.ProductName!.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        if (page < 1) page = 1;
        if (page > totalPages && totalPages > 0) page = totalPages;

        var products = await query
            .OrderBy(p => p.ProductName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewData["SearchTerm"] = search;
        ViewData["CurrentPage"] = page;
        ViewData["TotalPages"] = totalPages;
        ViewData["TotalCount"] = totalCount;
        return View(products);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return NotFound();
        return View(product);
    }

    public IActionResult Create()
    {
        ViewBag.Categories = _context.Categories.OrderBy(c => c.CategoryName).ToList();
        ViewBag.Suppliers = _context.Suppliers.OrderBy(s => s.CompanyName).ToList();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product product)
    {
        if (ModelState.IsValid)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            TempData["ToastSuccess"] = "Product created successfully!";
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Categories = _context.Categories.OrderBy(c => c.CategoryName).ToList();
        ViewBag.Suppliers = _context.Suppliers.OrderBy(s => s.CompanyName).ToList();
        return View(product);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();
        ViewBag.Categories = _context.Categories.OrderBy(c => c.CategoryName).ToList();
        ViewBag.Suppliers = _context.Suppliers.OrderBy(s => s.CompanyName).ToList();
        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Product product)
    {
        if (ModelState.IsValid)
        {
            _context.Update(product);
            await _context.SaveChangesAsync();
            TempData["ToastSuccess"] = "Product updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Categories = _context.Categories.OrderBy(c => c.CategoryName).ToList();
        ViewBag.Suppliers = _context.Suppliers.OrderBy(s => s.CompanyName).ToList();
        return View(product);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();
        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["ToastSuccess"] = "Product deleted successfully!";
        }
        return RedirectToAction(nameof(Index));
    }
}