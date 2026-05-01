using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebActionResults.Models;

namespace WebActionResults.Controllers;

public class SupplierController : Controller
{
    private readonly ShopDbContext _context;

    public SupplierController(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var suppliers = await _context.Suppliers
            .AsNoTracking()
            .Include(s => s.Products)
            .OrderBy(s => s.CompanyName)
            .ToListAsync();

        return View(suppliers);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var supplier = await _context.Suppliers
            .AsNoTracking()
            .Include(s => s.Products)
            .FirstOrDefaultAsync(s => s.SupplierID == id);

        if (supplier == null)
        {
            return NotFound();
        }

        return View(supplier);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("SupplierID,CompanyName,Phone")] Supplier supplier)
    {
        if (!ModelState.IsValid)
        {
            return View(supplier);
        }

        _context.Add(supplier);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Da them nha cung cap thanh cong.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier == null)
        {
            return NotFound();
        }

        return View(supplier);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("SupplierID,CompanyName,Phone")] Supplier supplier)
    {
        if (id != supplier.SupplierID)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(supplier);
        }

        try
        {
            _context.Update(supplier);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Da cap nhat nha cung cap thanh cong.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await SupplierExistsAsync(supplier.SupplierID))
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

        var supplier = await _context.Suppliers
            .AsNoTracking()
            .Include(s => s.Products)
            .FirstOrDefaultAsync(s => s.SupplierID == id);

        if (supplier == null)
        {
            return NotFound();
        }

        return View(supplier);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var supplier = await _context.Suppliers
            .Include(s => s.Products)
            .FirstOrDefaultAsync(s => s.SupplierID == id);

        if (supplier == null)
        {
            TempData["ErrorMessage"] = "Khong tim thay nha cung cap can xoa.";
            return RedirectToAction(nameof(Index));
        }

        if (supplier.Products.Any())
        {
            TempData["ErrorMessage"] = "Khong the xoa nha cung cap dang duoc gan cho san pham.";
            return RedirectToAction(nameof(Index));
        }

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Da xoa nha cung cap thanh cong.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> SupplierExistsAsync(int id)
    {
        return await _context.Suppliers.AnyAsync(e => e.SupplierID == id);
    }
}
