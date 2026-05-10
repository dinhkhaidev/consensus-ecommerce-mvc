using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebActionResults.Data.Entities;
using WebActionResults.Data.Services;
using WebActionResults.Services;
using WebActionResults.ViewModels;
using WebActionResults.Models;

namespace WebActionResults.Controllers;

public class AccountController : Controller
{
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;

    public AccountController(IUserService userService, IEmailService emailService)
    {
        _userService = userService;
        _emailService = emailService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        ViewData["ReturnUrl"] = model.ReturnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var result = await _userService.LoginAsync(model.UserName, model.Password, model.RememberMe);

        if (result.Succeeded)
        {
            // Check email verification (skip for admin)
            var user = await _userService.GetByUserNameAsync(model.UserName);
            var isAdmin = user?.UserName?.ToLower() == "admin";
            if (user != null && user.IsEmailVerified == false && !isAdmin)
            {
                await _userService.LogoutAsync();
                TempData["Warning"] = "Please verify your email first. A new verification link has been sent.";
                var token = await _userService.GenerateEmailVerificationTokenAsync(user.Email);
                await SendVerificationEmail(user.Email, token);
                return RedirectToAction("Login");
            }

            TempData["ToastSuccess"] = "Login successful.";
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            // Check user role and redirect accordingly
            var userName = user?.UserName?.ToLower() ?? "";
            if (userName == "admin")
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

            // Default redirect for customers
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "Invalid username or password.");
        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var existingUser = await _userService.GetByUserNameAsync(model.UserName);
        if (existingUser != null)
        {
            ModelState.AddModelError(nameof(model.UserName), "Username already exists.");
            return View(model);
        }

        var existingEmail = await _userService.GetByEmailAsync(model.Email);
        if (existingEmail != null)
        {
            ModelState.AddModelError(nameof(model.Email), "Email already in use.");
            return View(model);
        }

        var user = new Account
        {
            UserName = model.UserName,
            Email = model.Email,
            FullName = model.FullName,
            Phone = model.PhoneNumber ?? "",
            Birthday = model.Birthday
        };

        var result = await _userService.RegisterAsync(user, model.Password);

        if (result.Succeeded)
        {
            // Generate and send email verification
            var token = await _userService.GenerateEmailVerificationTokenAsync(user.Email);
            await SendVerificationEmail(user.Email, token);

            TempData["ToastSuccess"] = "Registration successful! Please check your email to verify your account.";
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error);

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _userService.LogoutAsync();
        TempData["ToastSuccess"] = "You have been logged out.";
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userService.GetCurrentUserAsync();
        if (user == null)
            return RedirectToAction("Login");

        var addresses = await _userService.GetAddressesAsync(user.Id);
        var model = ProfileViewModel.FromEntity(user);
        model.Addresses = addresses.Select(AddressViewModel.FromEntity).ToList();

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Profile", model);

        var user = await _userService.GetByIdAsync(model.Id);
        if (user == null)
            return RedirectToAction("Login");

        user.FullName = model.FullName;
        user.Phone = model.PhoneNumber ?? "";
        user.Birthday = model.Birthday;

        var result = await _userService.UpdateUserAsync(user);

        if (result.Succeeded)
        {
            TempData["ToastSuccess"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error);

        model.Addresses = (await _userService.GetAddressesAsync(user.Id))
            .Select(AddressViewModel.FromEntity).ToList();

        return View("Profile", model);
    }

    [Authorize]
    [HttpGet]
    public IActionResult AddAddress()
    {
        return View(new AddressEditViewModel());
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAddress(AddressEditViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == null)
            return RedirectToAction("Login");

        var address = new Address
        {
            UserId = userId.Value,
            FullName = model.FullName,
            Phone = model.Phone,
            AddressLine = model.AddressLine,
            Ward = model.Ward,
            District = model.District,
            City = model.City,
            PostalCode = model.PostalCode,
            IsDefault = model.IsDefault
        };

        var result = await _userService.AddAddressAsync(address);

        if (result.Succeeded)
        {
            TempData["ToastSuccess"] = "Address added successfully.";
            return RedirectToAction(nameof(Profile));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error);

        return View(model);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> EditAddress(int id)
    {
        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == null)
            return RedirectToAction("Login");

        var addresses = await _userService.GetAddressesAsync(userId.Value);
        var address = addresses.FirstOrDefault(a => a.Id == id);

        if (address == null)
            return RedirectToAction(nameof(Profile));

        return View(address);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAddress(AddressEditViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == null)
            return RedirectToAction("Login");

        var addresses = await _userService.GetAddressesAsync(userId.Value);
        var existingAddress = addresses.FirstOrDefault(a => a.Id == model.Id);

        if (existingAddress == null)
            return RedirectToAction(nameof(Profile));

        existingAddress.FullName = model.FullName;
        existingAddress.Phone = model.Phone;
        existingAddress.AddressLine = model.AddressLine;
        existingAddress.Ward = model.Ward;
        existingAddress.District = model.District;
        existingAddress.City = model.City;
        existingAddress.PostalCode = model.PostalCode;
        existingAddress.IsDefault = model.IsDefault;

        var result = await _userService.UpdateAddressAsync(existingAddress);

        if (result.Succeeded)
        {
            TempData["ToastSuccess"] = "Address updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error);

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == null)
            return RedirectToAction("Login");

        var result = await _userService.DeleteAddressAsync(userId.Value, id);

        if (result.Succeeded)
            TempData["ToastSuccess"] = "Address deleted.";
        else
            TempData["ToastError"] = "Failed to delete address.";

        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public IActionResult VerifyEmail(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            TempData["ToastError"] = "Invalid verification link.";
            return RedirectToAction("Login");
        }

        return View("VerifyEmail", new VerifyEmailViewModel { Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var success = await _userService.VerifyEmailAsync(model.Token);

        if (success)
        {
            TempData["ToastSuccess"] = "Email verified successfully! You can now login.";
            return RedirectToAction("Login");
        }

        TempData["ToastError"] = "Invalid or expired verification link.";
        return View(model);
    }

    private async Task SendVerificationEmail(string email, string token)
    {
        await _emailService.SendVerificationEmailAsync(email, token);
    }
}
