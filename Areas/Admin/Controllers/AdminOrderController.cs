using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebActionResults.Data.Entities;
using WebActionResults.Models;

namespace WebActionResults.Areas.Admin.Controllers;

[Area("Admin")]
public class AdminOrderController : AdminControllerBase
{
    private readonly ShopDbContext _context;

    public AdminOrderController(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? status, int page = 1, int pageSize = 15)
    {
        var query = _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
        {
            query = query.Where(o => o.Status == orderStatus);
        }

        var totalCount = await query.CountAsync();
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewData["StatusFilter"] = status;
        ViewData["CurrentPage"] = page;
        ViewData["PageSize"] = pageSize;
        ViewData["TotalCount"] = totalCount;
        ViewData["TotalPages"] = (int)Math.Ceiling(totalCount / (double)pageSize);

        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();
        return View(order);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order != null)
        {
            order.Status = status;
            await _context.SaveChangesAsync();
            TempData["ToastSuccess"] = "Order status updated!";
        }
        return RedirectToAction(nameof(Details), new { id });
    }
}