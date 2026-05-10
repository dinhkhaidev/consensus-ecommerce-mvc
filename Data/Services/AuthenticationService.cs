using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WebActionResults.Data.Entities;
using WebActionResults.Data.Repositories;
using WebActionResults.Models;

namespace WebActionResults.Data.Services;

public interface IAuthenticationService
{
    Task<Account?> LoginAsync(string userName, string password);
    Task LogoutAsync();
    Task<Account?> RegisterAsync(string userName, string email, string password, string fullName, string? phone, DateTime? birthday);
    Task<Account?> GetCurrentUserAsync();
    Task<int?> GetCurrentUserIdAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<List<Address>> GetAddressesAsync(int userId);
    Task<Address?> GetDefaultAddressAsync(int userId);
    Task<Address?> GetAddressByIdAsync(int addressId);
    Task<bool> AddAddressAsync(Address address);
    Task<bool> UpdateAddressAsync(Address address);
    Task<bool> DeleteAddressAsync(int userId, int addressId);
    Task<bool> UpdateAccountAsync(Account account);
    Task<Account?> GetAccountByIdAsync(int userId);
    Task<Account?> GetAccountByEmailAsync(string email);
    Task<Account?> GetAccountByUserNameAsync(string userName);
    Task<string> GenerateEmailVerificationTokenAsync(string email);
    Task<bool> VerifyEmailAsync(string token);
}

public class AuthenticationService : IAuthenticationService
{
    private const string SessionKey_UserId = "USER_ID";
    private const string SessionKey_UserName = "USER_NAME";
    private const string SessionKey_FullName = "FULL_NAME";

    private readonly ShopDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationService(ShopDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession Session => _httpContextAccessor.HttpContext!.Session;

    public async Task<Account?> LoginAsync(string userName, string password)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.UserName == userName);

        if (account == null)
            return null;

        // Verify password - in production, use proper hashing like BCrypt
        var hashedPassword = HashPassword(password);
        if (account.Password != hashedPassword)
            return null;

        // Check if account is active (Status = 1 means active)
        if (account.Status != 1)
            return null;

        // Store user info in session
        Session.SetInt32(SessionKey_UserId, account.Id);
        Session.SetString(SessionKey_UserName, account.UserName ?? "");
        Session.SetString(SessionKey_FullName, account.FullName ?? "");
        Session.SetString("USER_ROLE", account.UserName == "admin" ? "Admin" : "Customer");

        return account;
    }

    public Task LogoutAsync()
    {
        Session.Clear();
        return Task.CompletedTask;
    }

    public async Task<Account?> RegisterAsync(string userName, string email, string password, string fullName, string? phone, DateTime? birthday)
    {
        // Check if username already exists
        var existingUser = await _context.Accounts
            .FirstOrDefaultAsync(a => a.UserName == userName);
        if (existingUser != null)
            return null;

        // Check if email already exists
        var existingEmail = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Email == email);
        if (existingEmail != null)
            return null;

        var account = new Account
        {
            UserName = userName,
            Email = email,
            Password = HashPassword(password),
            FullName = fullName,
            Phone = phone ?? "",
            Birthday = birthday,
            Status = 1 // Active
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        // Auto login after registration
        Session.SetInt32(SessionKey_UserId, account.Id);
        Session.SetString(SessionKey_UserName, account.UserName ?? "");
        Session.SetString(SessionKey_FullName, account.FullName ?? "");

        return account;
    }

    public async Task<Account?> GetCurrentUserAsync()
    {
        var userId = Session.GetInt32(SessionKey_UserId);
        if (!userId.HasValue)
            return null;

        return await _context.Accounts.FindAsync(userId.Value);
    }

    public Task<int?> GetCurrentUserIdAsync()
    {
        var userId = Session.GetInt32(SessionKey_UserId);
        return Task.FromResult(userId);
    }

    public Task<bool> IsAuthenticatedAsync()
    {
        var userId = Session.GetInt32(SessionKey_UserId);
        return Task.FromResult(userId.HasValue);
    }

    public async Task<List<Address>> GetAddressesAsync(int userId)
    {
        return await _context.Addresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.Id)
            .ToListAsync();
    }

    public async Task<Address?> GetDefaultAddressAsync(int userId)
    {
        return await _context.Addresses
            .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault);
    }

    public async Task<Address?> GetAddressByIdAsync(int addressId)
    {
        return await _context.Addresses.FindAsync(addressId);
    }

    public async Task<bool> AddAddressAsync(Address address)
    {
        try
        {
            if (address.IsDefault)
            {
                var existingDefaults = await _context.Addresses
                    .Where(a => a.UserId == address.UserId && a.IsDefault)
                    .ToListAsync();
                foreach (var ad in existingDefaults)
                    ad.IsDefault = false;
            }
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateAddressAsync(Address address)
    {
        try
        {
            if (address.IsDefault)
            {
                var existingDefaults = await _context.Addresses
                    .Where(a => a.UserId == address.UserId && a.IsDefault && a.Id != address.Id)
                    .ToListAsync();
                foreach (var ad in existingDefaults)
                    ad.IsDefault = false;
            }
            _context.Addresses.Update(address);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteAddressAsync(int userId, int addressId)
    {
        try
        {
            var address = await _context.Addresses.FindAsync(addressId);
            if (address == null || address.UserId != userId)
                return false;

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateAccountAsync(Account account)
    {
        try
        {
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
            // Update session
            Session.SetString(SessionKey_FullName, account.FullName ?? "");
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Account?> GetAccountByIdAsync(int userId)
    {
        return await _context.Accounts.FindAsync(userId);
    }

    public async Task<Account?> GetAccountByEmailAsync(string email)
    {
        return await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
    }

    public async Task<Account?> GetAccountByUserNameAsync(string userName)
    {
        return await _context.Accounts.FirstOrDefaultAsync(a => a.UserName == userName);
    }

    public async Task<string> GenerateEmailVerificationTokenAsync(string email)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
        if (account == null)
            return string.Empty;

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        account.EmailVerificationToken = token;
        account.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
        await _context.SaveChangesAsync();

        return token;
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.EmailVerificationToken == token);

        if (account == null)
            return false;

        if (account.EmailVerificationTokenExpiresAt < DateTime.UtcNow)
            return false;

        account.IsEmailVerified = true;
        account.EmailVerificationToken = null;
        account.EmailVerificationTokenExpiresAt = null;
        await _context.SaveChangesAsync();

        return true;
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
