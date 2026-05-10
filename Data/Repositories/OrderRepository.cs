using Microsoft.EntityFrameworkCore;
using WebActionResults.Data.Entities;
using WebActionResults.Models;

namespace WebActionResults.Data.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int orderId);
    Task<Order?> GetByIdWithDetailsAsync(int orderId);
    Task<Order?> GetByOrderNumberAsync(string orderNumber);
    Task<List<Order>> GetByUserIdAsync(int userId);
    Task<Order> CreateAsync(Order order);
    Task UpdateAsync(Order order);
    Task<string> GenerateOrderNumberAsync();
}

public class OrderRepository : IOrderRepository
{
    private readonly ShopDbContext _context;

    public OrderRepository(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(int orderId)
        => await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);

    public async Task<Order?> GetByIdWithDetailsAsync(int orderId)
        => await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
            .Include(o => o.Address)
            .Include(o => o.Coupon)
            .FirstOrDefaultAsync(o => o.Id == orderId);

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
        => await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

    public async Task<List<Order>> GetByUserIdAsync(int userId)
        => await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task<Order> CreateAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task UpdateAsync(Order order)
    {
        order.UpdatedAt = DateTime.UtcNow;
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }

    public async Task<string> GenerateOrderNumberAsync()
    {
        var today = DateTime.UtcNow.Date;
        var prefix = $"ORD{today:yyyyMMdd}";

        var lastOrder = await _context.Orders
            .Where(o => o.OrderNumber.StartsWith(prefix))
            .OrderByDescending(o => o.OrderNumber)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastOrder != null)
        {
            var lastSeq = lastOrder.OrderNumber.Replace(prefix, "");
            if (int.TryParse(lastSeq, out int lastNum))
                sequence = lastNum + 1;
        }

        return $"{prefix}{sequence:D4}";
    }
}