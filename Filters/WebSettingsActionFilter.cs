using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebActionResults.Data.Services;

namespace WebActionResults.Filters;

public class WebSettingsActionFilter : IAsyncActionFilter
{
    private readonly IWebSettingsService _settingsService;

    public WebSettingsActionFilter(IWebSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.Controller is Controller controller)
        {
            controller.ViewData["WebSettings"] = await _settingsService.GetAllSettingsAsync();
        }

        await next();
    }
}
