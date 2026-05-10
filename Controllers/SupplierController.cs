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
            .FirstOrDefaultAsync(s => s.Id == id);

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
    public async Task<IActionResult> Create([Bind("Id,CompanyName,Phone")] Supplier supplier)
    {
        if (!ModelState.IsValid)
        {
            return View(supplier);
        }

        _context.Add(supplier);
        await _context.SaveChangesAsync();
        TempData["ToastSuccess"] = "Da them nha cung cap thanh cong.";
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
    public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyName,Phone")] Supplier supplier)
    {
        if (id != supplier.Id)
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
            TempData["ToastSuccess"] = "Da cap nhat nha cung cap thanh cong.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await SupplierExistsAsync(supplier.Id))
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
            .FirstOrDefaultAsync(s => s.Id == id);

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
            .FirstOrDefaultAsync(s => s.Id == id);

        if (supplier == null)
        {
            TempData["ToastError"] = "Khong tim thay nha cung cap can xoa.";
            return RedirectToAction(nameof(Index));
        }

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync();
        TempData["ToastSuccess"] = "Da xoa nha cung cap thanh cong.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> SupplierExistsAsync(int id)
    {
        return await _context.Suppliers.AnyAsync(e => e.Id == id);
    }
}