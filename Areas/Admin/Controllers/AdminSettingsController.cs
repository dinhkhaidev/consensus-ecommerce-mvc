using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebActionResults.Data.Services;

namespace WebActionResults.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(AuthenticationSchemes = "Session")]
public class AdminSettingsController : AdminControllerBase
{
    private readonly IWebSettingsService _settingsService;
    private readonly IWebHostEnvironment _environment;

    public AdminSettingsController(IWebSettingsService settingsService, IWebHostEnvironment environment)
    {
        _settingsService = settingsService;
        _environment = environment;
    }

    public async Task<IActionResult> Index()
    {
        var settings = await _settingsService.GetAllSettingsAsync();
        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(
        Dictionary<string, string>? settings,
        IFormFile? logoFile,
        IFormFile? faviconFile,
        IFormFile? openGraphImageFile)
    {
        settings ??= new Dictionary<string, string>();

        var logoUrl = await SaveSiteImageAsync(logoFile, "logo");
        if (!string.IsNullOrWhiteSpace(logoUrl))
            settings["LogoUrl"] = logoUrl;

        var faviconUrl = await SaveSiteImageAsync(faviconFile, "favicon");
        if (!string.IsNullOrWhiteSpace(faviconUrl))
            settings["FaviconUrl"] = faviconUrl;

        var openGraphImageUrl = await SaveSiteImageAsync(openGraphImageFile, "og");
        if (!string.IsNullOrWhiteSpace(openGraphImageUrl))
            settings["OpenGraphImageUrl"] = openGraphImageUrl;

        var booleanKeys = new[]
        {
            "MaintenanceMode",
            "EnableCOD",
            "EnableVNPay",
            "EnableMoMo",
            "EnableBankTransfer",
            "EnableReviews",
            "AutoApproveReviews",
            "EnableWishlist",
            "EnableNewsletterPopup",
            "EnableChatWidget",
            "ShowLowStockWarning",
            "RequireEmailVerification",
            "AnnouncementEnabled",
            "PopupEnabled",
            "HideClosedPageLinks",
            "PageHomeEnabled",
            "PageShopEnabled",
            "PageCategoriesEnabled",
            "PageRoom3DEnabled",
            "PageCartEnabled",
            "PageCheckoutEnabled",
            "PageWishlistEnabled",
            "PageOrdersEnabled",
            "PageAboutEnabled",
            "PageContactEnabled",
            "PagePoliciesEnabled"
        };

        foreach (var key in booleanKeys)
            settings[key] = settings.ContainsKey(key) ? "true" : "false";

        foreach (var kvp in settings)
        {
            await _settingsService.UpdateSettingAsync(kvp.Key, kvp.Value?.Trim() ?? string.Empty);
        }

        TempData["ToastSuccess"] = "Settings updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    private async Task<string?> SaveSiteImageAsync(IFormFile? file, string prefix)
    {
        if (file == null || file.Length == 0)
            return null;

        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp", ".gif", ".ico", ".svg"
        };

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !allowedExtensions.Contains(extension))
            return null;

        if (file.Length > 5 * 1024 * 1024)
            return null;

        var folder = Path.Combine(_environment.WebRootPath, "uploads", "site");
        Directory.CreateDirectory(folder);

        var fileName = $"{prefix}-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var path = Path.Combine(folder, fileName);

        await using var stream = System.IO.File.Create(path);
        await file.CopyToAsync(stream);

        return $"/uploads/site/{fileName}";
    }

    public async Task<IActionResult> GetSetting(string key)
    {
        var value = await _settingsService.GetSettingAsync(key);
        return Json(new { key, value });
    }

    public async Task<IActionResult> Payment()
    {
        var settings = await _settingsService.GetAllSettingsAsync();
        return View(settings);
    }

    [HttpPost]
    public async Task<IActionResult> UpdatePaymentSettings(
        string VNPayUrl,
        string VNPayMerchantId,
        string VNPayMerchantSecret,
        string MoMoUrl,
        string MoMoPartnerCode,
        string MoMoAccessKey,
        string MoMoSecretKey,
        string EnableCOD)
    {
        await _settingsService.UpdateSettingAsync("VNPayUrl", VNPayUrl);
        await _settingsService.UpdateSettingAsync("VNPayMerchantId", VNPayMerchantId);
        await _settingsService.UpdateSettingAsync("VNPayMerchantSecret", VNPayMerchantSecret);
        await _settingsService.UpdateSettingAsync("MoMoUrl", MoMoUrl);
        await _settingsService.UpdateSettingAsync("MoMoPartnerCode", MoMoPartnerCode);
        await _settingsService.UpdateSettingAsync("MoMoAccessKey", MoMoAccessKey);
        await _settingsService.UpdateSettingAsync("MoMoSecretKey", MoMoSecretKey);
        await _settingsService.UpdateSettingAsync("EnableCOD", EnableCOD == "on" ? "true" : "false");

        TempData["ToastSuccess"] = "Payment settings updated successfully!";
        return RedirectToAction(nameof(Payment));
    }
}
