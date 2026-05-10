using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebActionResults.Models;

namespace WebActionResults.Areas.Admin.Controllers;

[Area("Admin")]
public class DashboardController : AdminControllerBase
{
    private readonly ShopDbContext _context;

    public DashboardController(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var totalProducts = await _context.Products.CountAsync();
        var totalOrders = await _context.Orders.CountAsync();
        var totalRevenue = await _context.Orders.Where(o => o.PaymentStatus == Data.Entities.PaymentStatus.Paid).SumAsync(o => o.TotalAmount);
        var pendingOrders = await _context.Orders.CountAsync(o => o.Status == Data.Entities.OrderStatus.Pending);
        var totalCustomers = await _context.Accounts.CountAsync();
        var lowStockCount = await _context.ProductVariants.CountAsync(v => v.StockQuantity > 0 && v.StockQuantity <= 5);

        ViewData["TotalProducts"] = totalProducts;
        ViewData["TotalOrders"] = totalOrders;
        ViewData["TotalRevenue"] = totalRevenue;
        ViewData["PendingOrders"] = pendingOrders;
        ViewData["TotalCustomers"] = totalCustomers;
        ViewData["LowStockCount"] = lowStockCount;

        // Recent orders
        var recentOrders = await _context.Orders
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .ToListAsync();
        ViewBag.RecentOrders = recentOrders;

        // Top selling products (based on order items)
        var topProductsQuery = await _context.OrderItems
            .GroupBy(oi => oi.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                ProductName = g.First().Product != null ? g.First().Product.ProductName : "Unknown",
                TotalSold = g.Sum(oi => oi.Quantity),
                TotalRevenue = g.Sum(oi => oi.TotalPrice)
            })
            .OrderByDescending(x => x.TotalSold)
            .Take(5)
            .ToListAsync();
        ViewBag.TopProducts = topProductsQuery;

        return View();
    }
}