using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebActionResults.DTOs.Requests;
using WebActionResults.Models;

namespace WebActionResults.Controllers;

public class AccountController : Controller
{
    private readonly ShopDbContext _context;
    private readonly IMapper _mapper;

    public AccountController(ShopDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new Register());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(Register model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userName = model.UserName.Trim();
        var fullName = model.FullName.Trim();
        var email = model.Email.Trim();
        var phone = model.Phone.Trim();

        if (await _context.Accounts.AnyAsync(a => a.UserName == userName))
        {
            ModelState.AddModelError(nameof(model.UserName), "Username already exists.");
        }

        if (await _context.Accounts.AnyAsync(a => a.Email == email))
        {
            ModelState.AddModelError(nameof(model.Email), "Email is already in use.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var account = _mapper.Map<Account>(model);

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Register success.";
        return RedirectToAction(nameof(Register));
    }
}