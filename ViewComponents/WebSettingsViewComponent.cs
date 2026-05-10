using Microsoft.AspNetCore.Mvc;
using WebActionResults.Data.Services;

namespace WebActionResults.ViewComponents;

public class WebSettingsViewComponent : ViewComponent
{
    private readonly IWebSettingsService _webSettingsService;

    public WebSettingsViewComponent(IWebSettingsService webSettingsService)
    {
        _webSettingsService = webSettingsService;
    }

    public async Task<IViewComponentResult> Invoke()
    {
        var settings = await _webSettingsService.GetAllSettingsAsync();
        return View(settings);
    }
}