using Microsoft.AspNetCore.Identity;
using WebActionResults.Data.Entities;
using WebActionResults.Data.Repositories;
using WebActionResults.Models;

namespace WebActionResults.Data.Services;

public class AuthResult
{
    public bool Succeeded { get; set; }
    public bool IsLockedOut { get; set; }
    public bool IsNotAllowed { get; set; }
    public List<string> Errors { get; set; } = new();
}

public interface IUserService
{
    Task<Account?> GetByIdAsync(int userId);
    Task<Account?> GetByEmailAsync(string email);
    Task<Account?> GetByUserNameAsync(string userName);
    Task<AuthResult> LoginAsync(string userName, string password, bool isPersistent);
    Task LogoutAsync();
    Task<AuthResult> RegisterAsync(Account user, string password);
    Task<AuthResult> UpdateUserAsync(Account user);
    Task<Address?> GetDefaultAddressAsync(int userId);
    Task<List<Address>> GetAddressesAsync(int userId);
    Task<AuthResult> AddAddressAsync(Address address);
    Task<AuthResult> UpdateAddressAsync(Address address);
    Task<AuthResult> DeleteAddressAsync(int userId, int addressId);
    Task<Account?> GetCurrentUserAsync();
    Task<int?> GetCurrentUserIdAsync();
    
    // Giao việc tạo và xác thực OTP
    Task<string> GenerateEmailVerificationTokenAsync(string email);
    Task<bool> VerifyEmailAsync(string email, string token);
}

public class UserService : IUserService
{
    private readonly IAuthenticationService _authService;

    public UserService(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<Account?> GetByIdAsync(int userId)
        => await _authService.GetAccountByIdAsync(userId);

    public async Task<Account?> GetByEmailAsync(string email)
        => await _authService.GetAccountByEmailAsync(email);

    public async Task<Account?> GetByUserNameAsync(string userName)
        => await _authService.GetAccountByUserNameAsync(userName);

    public async Task<AuthResult> LoginAsync(string userName, string password, bool isPersistent)
    {
        var user = await _authService.LoginAsync(userName, password);
        if (user != null)
        {
            return new AuthResult { Succeeded = true };
        }
        return new AuthResult
        {
            Succeeded = false,
            Errors = new List<string> { "Invalid username or password." }
        };
    }

    public async Task LogoutAsync()
        => await _authService.LogoutAsync();

    public async Task<AuthResult> RegisterAsync(Account user, string password)
    {
        var result = await _authService.RegisterAsync(
            user.UserName ?? "",
            user.Email ?? "",
            password,
            user.FullName ?? "",
            user.Phone,
            user.Birthday);

        if (result != null)
        {
            return new AuthResult { Succeeded = true };
        }
        return new AuthResult
        {
            Succeeded = false,
            Errors = new List<string> { "Registration failed. Username or email may already exist." }
        };
    }

    public async Task<AuthResult> UpdateUserAsync(Account user)
    {
        var success = await _authService.UpdateAccountAsync(user);
        return new AuthResult { Succeeded = success };
    }

    public async Task<Address?> GetDefaultAddressAsync(int userId)
        => await _authService.GetDefaultAddressAsync(userId);

    public async Task<List<Address>> GetAddressesAsync(int userId)
        => await _authService.GetAddressesAsync(userId);

    public async Task<AuthResult> AddAddressAsync(Address address)
    {
        var success = await _authService.AddAddressAsync(address);
        return new AuthResult { Succeeded = success };
    }

    public async Task<AuthResult> UpdateAddressAsync(Address address)
    {
        var success = await _authService.UpdateAddressAsync(address);
        return new AuthResult { Succeeded = success };
    }

    public async Task<AuthResult> DeleteAddressAsync(int userId, int addressId)
    {
        var success = await _authService.DeleteAddressAsync(userId, addressId);
        return new AuthResult { Succeeded = success };
    }

    public async Task<Account?> GetCurrentUserAsync()
        => await _authService.GetCurrentUserAsync();

    public async Task<int?> GetCurrentUserIdAsync()
        => await _authService.GetCurrentUserIdAsync();

    // ==========================================
    // CHUYỀN BÓNG SANG CHO AuthenticationService
    // ==========================================
    public async Task<string> GenerateEmailVerificationTokenAsync(string email)
        => await _authService.GenerateEmailVerificationTokenAsync(email);

    public async Task<bool> VerifyEmailAsync(string email, string token)
        => await _authService.VerifyEmailAsync(email, token);
}