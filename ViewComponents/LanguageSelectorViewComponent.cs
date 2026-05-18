using Microsoft.AspNetCore.Mvc;
using WebActionResults.Services;

namespace WebActionResults.ViewComponents;

public class LanguageSelectorViewComponent : ViewComponent
{
    private readonly ILocalizationService _localizationService;

    public LanguageSelectorViewComponent(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public IViewComponentResult Invoke()
    {
        var currentLang = HttpContext.Session.GetString("Language") ?? "en";
        ViewBag.CurrentLanguage = currentLang;
        return View();
    }
}
