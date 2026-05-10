using Microsoft.EntityFrameworkCore;
using WebActionResults.Data.Entities;
using WebActionResults.Models;

namespace WebActionResults.Data.Repositories;

public interface IShipmentRepository
{
    Task<Shipment?> GetByIdAsync(int id);
    Task<Shipment?> GetByOrderIdAsync(int orderId);
    Task<List<Shipment>> GetAllAsync();
    Task<List<Shipment>> GetPendingAsync();
    Task<Shipment> CreateAsync(Shipment shipment);
    Task UpdateAsync(Shipment shipment);
}

public class ShipmentRepository : IShipmentRepository
{
    private readonly ShopDbContext _context;

    public ShipmentRepository(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<Shipment?> GetByIdAsync(int id)
        => await _context.Shipments.FindAsync(id);

    public async Task<Shipment?> GetByOrderIdAsync(int orderId)
        => await _context.Shipments.FirstOrDefaultAsync(s => s.OrderId == orderId);

    public async Task<List<Shipment>> GetAllAsync()
        => await _context.Shipments
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

    public async Task<List<Shipment>> GetPendingAsync()
        => await _context.Shipments
            .Where(s => s.Status != ShipmentStatus.Delivered &&
                        s.Status != ShipmentStatus.Failed &&
                        s.Status != ShipmentStatus.Returned)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync();

    public async Task<Shipment> CreateAsync(Shipment shipment)
    {
        _context.Shipments.Add(shipment);
        await _context.SaveChangesAsync();
        return shipment;
    }

    public async Task UpdateAsync(Shipment shipment)
    {
        shipment.UpdatedAt = DateTime.UtcNow;
        _context.Shipments.Update(shipment);
        await _context.SaveChangesAsync();
    }
}