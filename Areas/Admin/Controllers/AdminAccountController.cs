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
    public async Task<IActionResult> Create(Account model, string password)
    {
        if (!ModelState.IsValid)
            return View(model);

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
    public async Task<IActionResult> Edit(Account model, string? password)
    {
        var account = await _context.Accounts.FindAsync(model.Id);
        if (account == null) return NotFound();

        if (!ModelState.IsValid)
            return View(model);

        account.FullName = model.FullName;
        account.Email = model.Email;
        account.Phone = model.Phone ?? "";
        account.Status = model.Status;
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
}
