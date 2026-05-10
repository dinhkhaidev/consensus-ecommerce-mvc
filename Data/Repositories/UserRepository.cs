using Microsoft.EntityFrameworkCore;
using WebActionResults.Data.Entities;
using WebActionResults.Models;

namespace WebActionResults.Data.Repositories;

public interface IUserRepository
{
    Task<Account?> GetByIdAsync(int userId);
    Task<Account?> GetByEmailAsync(string email);
    Task<Account?> GetByUserNameAsync(string userName);
    Task<bool> ExistsAsync(int userId);
}

public class UserRepository : IUserRepository
{
    private readonly ShopDbContext _context;

    public UserRepository(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByIdAsync(int userId)
        => await _context.Accounts.FindAsync(userId);

    public async Task<Account?> GetByEmailAsync(string email)
        => await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);

    public async Task<Account?> GetByUserNameAsync(string userName)
        => await _context.Accounts.FirstOrDefaultAsync(a => a.UserName == userName);

    public async Task<bool> ExistsAsync(int userId)
        => await _context.Accounts.AnyAsync(a => a.Id == userId);
}
