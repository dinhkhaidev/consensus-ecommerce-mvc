using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebActionResults.Data.Entities;
using WebActionResults.Models;

namespace WebActionResults.Areas.Admin.Controllers;

[Area("Admin")]
public class AdminCouponController : AdminControllerBase
{
    private readonly ShopDbContext _context;

    public AdminCouponController(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? status, int page = 1, int pageSize = 10, string search = "")
    {
        var query = _context.Coupons.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Code!.Contains(search) || c.Description!.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        if (page < 1) page = 1;
        if (page > totalPages && totalPages > 0) page = totalPages;

        var coupons = await query
            .OrderBy(c => c.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewData["SearchTerm"] = search;
        ViewData["CurrentPage"] = page;
        ViewData["TotalPages"] = totalPages;
        ViewData["TotalCount"] = totalCount;
        return View(coupons);
    }

    public async Task<IActionResult> Details(int id)
    {
        var coupon = await _context.Coupons
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (coupon == null) return NotFound();
        return View(coupon);
    }

    public IActionResult Create()
    {
        return View(new Coupon());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Coupon model)
    {
        if (ModelState.IsValid)
        {
            _context.Coupons.Add(model);
            await _context.SaveChangesAsync();
            TempData["ToastSuccess"] = "Coupon created successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var coupon = await _context.Coupons.FindAsync(id);
        if (coupon == null) return NotFound();
        return View(coupon);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Coupon model)
    {
        if (ModelState.IsValid)
        {
            _context.Coupons.Update(model);
            await _context.SaveChangesAsync();
            TempData["ToastSuccess"] = "Coupon updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var coupon = await _context.Coupons.FindAsync(id);
        if (coupon != null)
        {
            _context.Coupons.Remove(coupon);
            await _context.SaveChangesAsync();
            TempData["ToastSuccess"] = "Coupon deleted successfully!";
        }
        return RedirectToAction(nameof(Index));
    }
}
