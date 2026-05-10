using Microsoft.EntityFrameworkCore;
using WebActionResults.Data.Entities;
using WebActionResults.Models;

namespace WebActionResults.Data.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(int id);
    Task<Payment?> GetByOrderIdAsync(int orderId);
    Task<Payment?> GetByTransactionIdAsync(string transactionId);
    Task<Payment> CreateAsync(Payment payment);
    Task UpdateAsync(Payment payment);
}

public class PaymentRepository : IPaymentRepository
{
    private readonly ShopDbContext _context;

    public PaymentRepository(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(int id)
        => await _context.Payments.FindAsync(id);

    public async Task<Payment?> GetByOrderIdAsync(int orderId)
        => await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);

    public async Task<Payment?> GetByTransactionIdAsync(string transactionId)
        => await _context.Payments.FirstOrDefaultAsync(p => p.TransactionId == transactionId);

    public async Task<Payment> CreateAsync(Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task UpdateAsync(Payment payment)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();
    }
}