using Microsoft.EntityFrameworkCore;
using WebActionResults.Data.Entities;
using WebActionResults.Models;

namespace WebActionResults.Data.Repositories;

public interface ICouponRepository
{
    Task<Coupon?> GetByCodeAsync(string code);
    Task<Coupon?> GetByIdAsync(int id);
    Task<List<Coupon>> GetActiveCouponsAsync();
    Task UpdateAsync(Coupon coupon);
    Task<bool> IncrementUsageAsync(int couponId);
    Task DecrementUsageAsync(int couponId);
}

public class CouponRepository : ICouponRepository
{
    private readonly ShopDbContext _context;

    public CouponRepository(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<Coupon?> GetByCodeAsync(string code)
        => await _context.Coupons.FirstOrDefaultAsync(c => c.Code.ToUpper() == code.ToUpper() && c.IsActive);

    public async Task<Coupon?> GetByIdAsync(int id)
        => await _context.Coupons.FindAsync(id);

    public async Task<List<Coupon>> GetActiveCouponsAsync()
        => await _context.Coupons
            .Where(c => c.IsActive && c.StartDate <= DateTime.UtcNow && c.EndDate >= DateTime.UtcNow)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

    public async Task UpdateAsync(Coupon coupon)
    {
        _context.Coupons.Update(coupon);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IncrementUsageAsync(int couponId)
    {
        var now = DateTime.UtcNow;
        var updatedRows = await _context.Coupons
            .Where(c => c.Id == couponId &&
                        c.IsActive &&
                        c.StartDate <= now &&
                        c.EndDate >= now &&
                        c.UsedCount < c.UsageLimit)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.UsedCount, c => c.UsedCount + 1));

        return updatedRows > 0;
    }

    public async Task DecrementUsageAsync(int couponId)
    {
        await _context.Coupons
            .Where(c => c.Id == couponId && c.UsedCount > 0)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.UsedCount, c => c.UsedCount - 1));
    }
}
