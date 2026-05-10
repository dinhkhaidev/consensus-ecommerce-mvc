using Microsoft.AspNetCore.Mvc;
using WebActionResults.Data.Services;

namespace WebActionResults.Areas.Admin.Controllers;

[Area("Admin")]
public class AdminWebSettingsController : AdminControllerBase
{
    private readonly IWebSettingsService _webSettingsService;

    public AdminWebSettingsController(IWebSettingsService webSettingsService)
    {
        _webSettingsService = webSettingsService;
    }

    public async Task<IActionResult> Index()
    {
        var settings = await _webSettingsService.GetAllSettingsAsync();
        return View(settings);
    }

    [HttpPost]
    public async Task<IActionResult> Update(Dictionary<string, string> settings)
    {
        foreach (var kvp in settings)
        {
            await _webSettingsService.UpdateSettingAsync(kvp.Key, kvp.Value);
        }
        TempData["ToastSuccess"] = "Settings updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Payment()
    {
        var settings = await _webSettingsService.GetAllSettingsAsync();
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
        await _webSettingsService.UpdateSettingAsync("VNPayUrl", VNPayUrl);
        await _webSettingsService.UpdateSettingAsync("VNPayMerchantId", VNPayMerchantId);
        await _webSettingsService.UpdateSettingAsync("VNPayMerchantSecret", VNPayMerchantSecret);
        await _webSettingsService.UpdateSettingAsync("MoMoUrl", MoMoUrl);
        await _webSettingsService.UpdateSettingAsync("MoMoPartnerCode", MoMoPartnerCode);
        await _webSettingsService.UpdateSettingAsync("MoMoAccessKey", MoMoAccessKey);
        await _webSettingsService.UpdateSettingAsync("MoMoSecretKey", MoMoSecretKey);
        await _webSettingsService.UpdateSettingAsync("EnableCOD", EnableCOD == "on" ? "true" : "false");

        TempData["ToastSuccess"] = "Payment settings updated successfully!";
        return RedirectToAction(nameof(Payment));
    }
}