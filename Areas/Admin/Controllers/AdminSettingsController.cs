using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebActionResults.Data.Services;

namespace WebActionResults.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(AuthenticationSchemes = "Session")]
public class AdminSettingsController : AdminControllerBase
{
    private readonly IWebSettingsService _settingsService;

    public AdminSettingsController(IWebSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<IActionResult> Index()
    {
        var settings = await _settingsService.GetAllSettingsAsync();
        return View(settings);
    }

    [HttpPost]
    public async Task<IActionResult> Update(Dictionary<string, string> settings)
    {
        foreach (var kvp in settings)
        {
            await _settingsService.UpdateSettingAsync(kvp.Key, kvp.Value);
        }

        TempData["Success"] = "Settings updated successfully!";
        return RedirectToAction(nameof(Index));
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