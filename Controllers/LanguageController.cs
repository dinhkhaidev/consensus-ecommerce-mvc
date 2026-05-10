using Microsoft.AspNetCore.Mvc;
using WebActionResults.Services;

namespace WebActionResults.Controllers;

public class LanguageController : Controller
{
    private readonly ILocalizationService _localizationService;

    public LanguageController(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public IActionResult Switch(string lang, string? returnUrl = null)
    {
        _localizationService.SetLanguage(lang);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }
}