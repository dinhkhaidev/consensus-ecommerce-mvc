using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebActionResults.Models;

namespace WebActionResults.Controllers;

public class ProductController : Controller
{
    private readonly ShopDbContext _context;

    public ProductController(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? categoryId)
    {
        IQueryable<Product> productsQuery = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Supplier);

        if (categoryId.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.CategoryID == categoryId.Value);

            ViewData["SelectedCategoryName"] = await _context.Categories
                .AsNoTracking()
                .Where(c => c.CategoryID == categoryId.Value)
                .Select(c => c.CategoryName)
                .FirstOrDefaultAsync();
        }

        var products = await productsQuery
            .OrderBy(p => p.ProductName)
            .ToListAsync();

        return View(products);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.ProductID == id);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateSelectListsAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ProductID,ProductName,SupplierID,CategoryID,QuantityPerUnit,UnitPrice,Discontinued")] Product product)
    {
        // Ignore validation for navigation properties that are not posted by the form.
        ModelState.Remove(nameof(Product.Category));
        ModelState.Remove(nameof(Product.Supplier));

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(product.CategoryID, product.SupplierID);
            return View(product);
        }

        _context.Add(product);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Da them san pham thanh cong.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        await PopulateSelectListsAsync(product.CategoryID, product.SupplierID);
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ProductID,ProductName,SupplierID,CategoryID,QuantityPerUnit,UnitPrice,Discontinued")] Product product)
    {
        // Ignore validation for navigation properties that are not posted by the form.
        ModelState.Remove(nameof(Product.Category));
        ModelState.Remove(nameof(Product.Supplier));

        if (id != product.ProductID)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(product.CategoryID, product.SupplierID);
            return View(product);
        }

        try
        {
            _context.Update(product);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Da cap nhat san pham thanh cong.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ProductExistsAsync(product.ProductID))
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

        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.ProductID == id);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            TempData["ErrorMessage"] = "Khong tim thay san pham can xoa.";
            return RedirectToAction(nameof(Index));
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Da xoa san pham thanh cong.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateSelectListsAsync(int? selectedCategoryId = null, int? selectedSupplierId = null)
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.CategoryName)
            .ToListAsync();

        var suppliers = await _context.Suppliers
            .AsNoTracking()
            .OrderBy(s => s.CompanyName)
            .ToListAsync();

        ViewData["CategoryID"] = new SelectList(categories, "CategoryID", "CategoryName", selectedCategoryId);
        ViewData["SupplierID"] = new SelectList(suppliers, "SupplierID", "CompanyName", selectedSupplierId);
    }

    private async Task<bool> ProductExistsAsync(int id)
    {
        return await _context.Products.AnyAsync(e => e.ProductID == id);
    }
}
