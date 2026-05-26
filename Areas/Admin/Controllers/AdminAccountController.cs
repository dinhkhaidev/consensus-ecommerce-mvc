using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebActionResults.Models;

namespace WebActionResults.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(AuthenticationSchemes = "Session")]
public class AdminAccountController : AdminControllerBase
{
    private readonly ShopDbContext _context;

    public AdminAccountController(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string search = "")
    {
        await NormalizeAccountDateColumnsAsync();

        var query = _context.Accounts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(a => a.UserName!.Contains(search) || a.Email!.Contains(search) || a.FullName!.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        if (page < 1) page = 1;
        if (page > totalPages && totalPages > 0) page = totalPages;

        var accounts = await query
            .OrderBy(a => a.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewData["SearchTerm"] = search;
        ViewData["CurrentPage"] = page;
        ViewData["TotalPages"] = totalPages;
        ViewData["TotalCount"] = totalCount;
        return View(accounts);
    }

    public async Task<IActionResult> Details(int id)
    {
        await NormalizeAccountDateColumnsAsync();

        var account = await _context.Accounts
            .Include(a => a.Addresses)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (account == null) return NotFound();
        return View(account);
    }

    public IActionResult Create()
    {
        return View(new Account());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Account model, string password, string? adminConfirmPassword, string? adminSecretCode)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Verify admin password + secret code when assigning Admin role
        if (string.Equals(model.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            var verifyResult = await VerifyAdminAssignmentAsync(adminConfirmPassword, adminSecretCode);
            if (verifyResult != null)
            {
                ModelState.AddModelError("", verifyResult);
                return View(model);
            }
        }

        if (await _context.Accounts.AnyAsync(a => a.UserName == model.UserName))
        {
            ModelState.AddModelError(nameof(model.UserName), "Username already exists.");
            return View(model);
        }

        if (await _context.Accounts.AnyAsync(a => a.Email == model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "Email already exists.");
            return View(model);
        }

        var account = new Account
        {
            UserName = model.UserName,
            Email = model.Email,
            FullName = model.FullName,
            Phone = model.Phone ?? "",
            Password = HashPassword(password),
            Status = 1,
            Role = model.Role ?? "Customer",
            CreatedAt = DateTime.UtcNow,
            IsEmailVerified = true
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        TempData["ToastSuccess"] = "Account created successfully!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account == null) return NotFound();
        return View(account);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Account model, string? password, string? adminConfirmPassword, string? adminSecretCode)
    {
        var account = await _context.Accounts.FindAsync(model.Id);
        if (account == null) return NotFound();

        if (!ModelState.IsValid)
            return View(model);

        // Verify admin password + secret code when promoting to Admin role
        if (string.Equals(model.Role, "Admin", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(account.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            var verifyResult = await VerifyAdminAssignmentAsync(adminConfirmPassword, adminSecretCode);
            if (verifyResult != null)
            {
                ModelState.AddModelError("", verifyResult);
                return View(model);
            }
        }

        account.FullName = model.FullName;
        account.Email = model.Email;
        account.Phone = model.Phone ?? "";
        account.Status = model.Status;
        account.Role = model.Role ?? "Customer";
        account.IsEmailVerified = model.IsEmailVerified;

        if (!string.IsNullOrWhiteSpace(password))
        {
            account.Password = HashPassword(password);
        }

        await _context.SaveChangesAsync();
        TempData["ToastSuccess"] = "Account updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account != null)
        {
            var hasOrders = await _context.Orders.AnyAsync(o => o.UserId == id);
            if (hasOrders)
            {
                account.Status = 0;
                account.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["ToastWarning"] = "Account has order history, so it was deactivated instead of deleted.";
                return RedirectToAction(nameof(Index));
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            TempData["ToastSuccess"] = "Account deleted successfully!";
        }
        return RedirectToAction(nameof(Index));
    }

    private static string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private async Task<string?> VerifyAdminAssignmentAsync(string? confirmPassword, string? secretCode)
    {
        // 1. Verify admin password
        if (string.IsNullOrWhiteSpace(confirmPassword))
            return "Admin password is required to assign Admin role.";

        var currentUserId = HttpContext.Session.GetInt32("USER_ID");
        if (!currentUserId.HasValue)
            return "Session expired. Please login again.";

        var currentAdmin = await _context.Accounts.FindAsync(currentUserId.Value);
        if (currentAdmin == null)
            return "Current admin account not found.";

        if (currentAdmin.Password != HashPassword(confirmPassword))
            return "Invalid admin password.";

        // 2. Verify secret code from environment
        var envSecretCode = Environment.GetEnvironmentVariable("ADMIN_SECRET_CODE") ?? "";
        if (!string.IsNullOrEmpty(envSecretCode) && !string.Equals(secretCode, envSecretCode, StringComparison.Ordinal))
            return "Invalid security code.";

        return null; // All checks passed
    }

    private async Task NormalizeAccountDateColumnsAsync()
    {
        await _context.Database.ExecuteSqlRawAsync("""
            UPDATE Account
            SET CreatedAt = SYSUTCDATETIME()
            WHERE CreatedAt IS NULL
            """);
    }
}
