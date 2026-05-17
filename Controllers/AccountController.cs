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
    private readonly ILocalizationService _localizer;
    private readonly IWebSettingsService _settingsService;
    private readonly IWebHostEnvironment _environment;

    public AccountController(IUserService userService, IEmailService emailService, ILocalizationService localizer, IWebSettingsService settingsService, IWebHostEnvironment environment)
    {
        _userService = userService;
        _emailService = emailService;
        _localizer = localizer;
        _settingsService = settingsService;
        _environment = environment;
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
        model.UserName = model.UserName?.Trim() ?? string.Empty;
        model.Password = model.Password ?? string.Empty;
        ViewData["ReturnUrl"] = model.ReturnUrl;

        ModelState.Clear();
        if (string.IsNullOrWhiteSpace(model.UserName))
            AddModelErrorKey(nameof(model.UserName), "Auth.UsernameOrEmailRequired");
        if (string.IsNullOrWhiteSpace(model.Password))
            AddModelErrorKey(nameof(model.Password), "Auth.PasswordRequired");

        if (!ModelState.IsValid)
            return View(model);

        var result = await _userService.LoginAsync(model.UserName, model.Password, model.RememberMe);

        if (result.Succeeded)
        {
            // Check email verification (skip for admin, skip if disabled)
            var user = await _userService.GetCurrentUserAsync();
            var isAdmin = string.Equals(user?.Role, "Admin", StringComparison.OrdinalIgnoreCase);
            var allSettings = await _settingsService.GetAllSettingsAsync();
            var requireVerification = string.Equals(allSettings.GetValueOrDefault("RequireEmailVerification", "false"), "true", StringComparison.OrdinalIgnoreCase);
            if (requireVerification && user != null && user.IsEmailVerified == false && !isAdmin)
            {
                await _userService.LogoutAsync();
                TempData["ToastWarningKey"] = "Auth.VerifyEmailFirst";
                var token = await _userService.GenerateEmailVerificationTokenAsync(user.Email);
                await SendVerificationEmail(user.Email, token);

                return RedirectToAction("VerifyEmail", new { email = user.Email });
            }

            TempData["ToastSuccessKey"] = "Auth.LoginSuccessful";
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            // Check user role and redirect accordingly
            var userRole = HttpContext.Session.GetString("USER_ROLE");
            if (string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

            // Default redirect for customers
            return RedirectToAction("Index", "Home");
        }

        AddModelErrorKey("", "Auth.InvalidLogin");
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
        model.UserName = model.UserName?.Trim() ?? string.Empty;
        model.Email = model.Email?.Trim() ?? string.Empty;
        model.FullName = model.FullName?.Trim() ?? string.Empty;
        model.PhoneNumber = model.PhoneNumber?.Trim() ?? string.Empty;

        ValidateRegister(model);

        if (!ModelState.IsValid)
            return View(model);

        var existingUser = await _userService.GetByUserNameAsync(model.UserName);
        if (existingUser != null)
        {
            AddModelErrorKey(nameof(model.UserName), "Auth.UsernameExists");
            return View(model);
        }

        var existingEmail = await _userService.GetByEmailAsync(model.Email);
        if (existingEmail != null)
        {
            AddModelErrorKey(nameof(model.Email), "Auth.EmailInUse");
            return View(model);
        }

        var user = new Account
        {
            UserName = model.UserName,
            Email = model.Email,
            FullName = model.FullName,
            Phone = model.PhoneNumber,
            Birthday = model.Birthday
        };

        var result = await _userService.RegisterAsync(user, model.Password);

        if (result.Succeeded)
        {
            // Generate and send email verification
            var token = await _userService.GenerateEmailVerificationTokenAsync(user.Email);
            await SendVerificationEmail(user.Email, token);

            TempData["ToastSuccessKey"] = "Auth.RegisterSuccessfulVerifyEmail";
            return RedirectToAction("Login");
            TempData["ToastSuccess"] = "Registration successful! Please check your email to verify your account.";
            return RedirectToAction("VerifyEmail", new { email = user.Email });
        }

        AddModelErrorKey("", "Auth.RegisterFailed");

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _userService.LogoutAsync();
        TempData["ToastSuccessKey"] = "Auth.LogoutSuccessful";
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
    public async Task<IActionResult> UpdateProfile(ProfileViewModel model, IFormFile? avatarFile)
    {
        if (!ModelState.IsValid)
            return View("Profile", model);

        var user = await _userService.GetByIdAsync(model.Id);
        if (user == null)
            return RedirectToAction("Login");

        user.FullName = model.FullName;
        user.Phone = model.PhoneNumber ?? "";
        user.Birthday = model.Birthday;
        var avatarUrl = await SaveAvatarAsync(avatarFile);
        if (!string.IsNullOrWhiteSpace(avatarUrl))
            user.AvatarUrl = avatarUrl;

        var result = await _userService.UpdateUserAsync(user);

        if (result.Succeeded)
        {
            TempData["ToastSuccessKey"] = "Auth.ProfileUpdated";
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
            TempData["ToastSuccessKey"] = "Auth.AddressAdded";
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

        return View(AddressEditViewModel.FromEntity(address));
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
            TempData["ToastSuccessKey"] = "Auth.AddressUpdated";
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
            TempData["ToastSuccessKey"] = "Auth.AddressDeleted";
        else
            TempData["ToastErrorKey"] = "Auth.AddressDeleteFailed";

        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public IActionResult VerifyEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            TempData["ToastErrorKey"] = "Auth.InvalidVerificationLink";
            return RedirectToAction("Login");
        }

        return View("VerifyEmail", new VerifyEmailViewModel { Email = email });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var success = await _userService.VerifyEmailAsync(model.Email, model.Token);

        if (success)
        {
            TempData["ToastSuccessKey"] = "Auth.EmailVerified";
            return RedirectToAction("Login");
        }

        TempData["ToastErrorKey"] = "Auth.VerificationLinkExpired";
        return View(model);
    }

    private async Task SendVerificationEmail(string email, string token)
    {
        await _emailService.SendVerificationEmailAsync(email, token);
    }

    private async Task<string?> SaveAvatarAsync(IFormFile? avatarFile)
    {
        if (avatarFile == null || avatarFile.Length == 0)
            return null;

        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp", ".gif"
        };

        var extension = Path.GetExtension(avatarFile.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !allowedExtensions.Contains(extension))
            return null;

        if (avatarFile.Length > 3 * 1024 * 1024)
            return null;

        var folder = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var path = Path.Combine(folder, fileName);

        await using var stream = System.IO.File.Create(path);
        await avatarFile.CopyToAsync(stream);

        return $"/uploads/avatars/{fileName}";
    }

    private void ValidateRegister(RegisterViewModel model)
    {
        ModelState.Clear();

        if (string.IsNullOrWhiteSpace(model.UserName))
            AddModelErrorKey(nameof(model.UserName), "Auth.UsernameRequired");
        else
        {
            if (model.UserName.Length < 3 || model.UserName.Length > 20)
                AddModelErrorKey(nameof(model.UserName), "Auth.UsernameLength");
            if (!System.Text.RegularExpressions.Regex.IsMatch(model.UserName, @"^[a-zA-Z0-9_]+$"))
                AddModelErrorKey(nameof(model.UserName), "Auth.UsernameFormat");
        }

        if (string.IsNullOrWhiteSpace(model.Email))
            AddModelErrorKey(nameof(model.Email), "Auth.EmailRequired");
        else if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(model.Email))
            AddModelErrorKey(nameof(model.Email), "Auth.EmailInvalid");

        if (string.IsNullOrWhiteSpace(model.FullName))
            AddModelErrorKey(nameof(model.FullName), "Auth.FullNameRequired");
        else if (model.FullName.Length > 100)
            AddModelErrorKey(nameof(model.FullName), "Auth.FullNameMaxLength");

        if (string.IsNullOrWhiteSpace(model.Password))
            AddModelErrorKey(nameof(model.Password), "Auth.PasswordRequired");
        else if (model.Password.Length < 6)
            AddModelErrorKey(nameof(model.Password), "Auth.PasswordMinLength");

        if (string.IsNullOrWhiteSpace(model.ConfirmPassword))
            AddModelErrorKey(nameof(model.ConfirmPassword), "Auth.ConfirmPasswordRequired");
        else if (!string.Equals(model.Password, model.ConfirmPassword, StringComparison.Ordinal))
            AddModelErrorKey(nameof(model.ConfirmPassword), "Auth.PasswordsDoNotMatch");

        if (string.IsNullOrWhiteSpace(model.PhoneNumber))
            AddModelErrorKey(nameof(model.PhoneNumber), "Auth.PhoneRequired");
        else if (!System.Text.RegularExpressions.Regex.IsMatch(model.PhoneNumber, @"^0[39875]\d{8}$"))
            AddModelErrorKey(nameof(model.PhoneNumber), "Auth.PhoneInvalid");

        if (!model.Birthday.HasValue)
            AddModelErrorKey(nameof(model.Birthday), "Auth.BirthdayRequired");
        else if (!IsAtLeast16(model.Birthday.Value))
            AddModelErrorKey(nameof(model.Birthday), "Auth.MustBe16ToRegister");

        if (!model.TermsAccepted)
            AddModelErrorKey(nameof(model.TermsAccepted), "Auth.TermsRequired");
    }

    private void AddModelErrorKey(string field, string key)
    {
        ModelState.AddModelError(field, _localizer.Get(key));
    }

    private static bool IsAtLeast16(DateTime birthday)
    {
        var today = DateTime.UtcNow.Date;
        var cutoff = today.AddYears(-16);
        return birthday.Date <= cutoff;
    }

    

        [HttpGet]
    public async Task<IActionResult> VerifyEmailClick(string email, string token)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
        {
            TempData["ToastError"] = "Đường dẫn xác thực không hợp lệ hoặc đã hết hạn.";
            return RedirectToAction("Login");
        }

        // Thực hiện gọi service để kiểm tra DB và update trạng thái IsEmailVerified
        var success = await _userService.VerifyEmailAsync(email, token);

        if (success)
        {
            // Điều hướng sang trang báo thành công thiết kế riêng biệt
            return View("VerifySuccess");
        }

        TempData["ToastError"] = "Mã xác thực không chính xác hoặc đã hết hạn.";
        return RedirectToAction("Login");
    }
}
