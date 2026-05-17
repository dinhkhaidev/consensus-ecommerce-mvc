using System.Net;
using WebActionResults.Data.Services;

namespace WebActionResults.Middleware;

public class MaintenanceModeMiddleware
{
    private readonly RequestDelegate _next;

    public MaintenanceModeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldBypass(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var settingsService = context.RequestServices.GetRequiredService<IWebSettingsService>();
        var maintenanceMode = await settingsService.GetSettingAsync("MaintenanceMode");

        if (!string.Equals(maintenanceMode, "true", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var settings = await settingsService.GetAllSettingsAsync();
        var title = settings.GetValueOrDefault("MaintenanceTitle", "Website under maintenance");
        var message = settings.GetValueOrDefault("MaintenanceMessage", "We are improving the shopping experience. Please come back soon.");
        var supportEmail = settings.GetValueOrDefault("MaintenanceSupportEmail", settings.GetValueOrDefault("SupportEmail", "support@furnish.com"));
        var estimatedBackAt = settings.GetValueOrDefault("MaintenanceEstimatedBackAt", "");
        var siteTitle = settings.GetValueOrDefault("SiteTitle", "Furnish");

        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.ContentType = "text/html; charset=utf-8";
        context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        context.Response.Headers.Pragma = "no-cache";
        context.Response.Headers.RetryAfter = "3600";

        await context.Response.WriteAsync($$"""
<!doctype html>
<html lang="vi">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta name="robots" content="noindex,nofollow">
    <title>{{HtmlEncode(title)}} - {{HtmlEncode(siteTitle)}}</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/css/bootstrap.min.css" rel="stylesheet">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.13.1/font/bootstrap-icons.css">
    <style>
        body { min-height: 100vh; background: #f6f3ef; color: #24211f; }
        .maintenance-shell { min-height: 100vh; display: grid; place-items: center; padding: 24px; }
        .maintenance-panel { max-width: 720px; width: 100%; background: #fff; border: 1px solid #eadfd2; border-radius: 8px; box-shadow: 0 18px 60px rgba(45, 35, 25, .12); }
        .maintenance-icon { width: 64px; height: 64px; display: grid; place-items: center; background: #fff4e8; color: #b86d26; border-radius: 8px; }
        .text-brand { color: #b86d26; }
    </style>
</head>
<body>
    <main class="maintenance-shell">
        <section class="maintenance-panel p-4 p-md-5">
            <div class="d-flex align-items-center gap-3 mb-4">
                <div class="maintenance-icon"><i class="bi bi-tools fs-2"></i></div>
                <div>
                    <div class="text-uppercase small text-brand fw-semibold">{{HtmlEncode(siteTitle)}}</div>
                    <h1 class="h3 fw-bold mb-0">{{HtmlEncode(title)}}</h1>
                </div>
            </div>
            <p class="lead text-muted mb-4">{{HtmlEncode(message)}}</p>
            {{RenderEstimatedTime(estimatedBackAt)}}
            <div class="border-top pt-4 mt-4 d-flex flex-column flex-sm-row justify-content-between gap-3">
                <span class="text-muted">Cần hỗ trợ đơn hàng?</span>
                <a class="fw-semibold text-decoration-none text-brand" href="mailto:{{HtmlEncode(supportEmail)}}">{{HtmlEncode(supportEmail)}}</a>
            </div>
        </section>
    </main>
</body>
</html>
""");
    }

    private static bool ShouldBypass(PathString path)
    {
        return path.StartsWithSegments("/Admin", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWithSegments("/Payment", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWithSegments("/Fulfillment", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWithSegments("/Account/Login", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWithSegments("/Account/Logout", StringComparison.OrdinalIgnoreCase);
    }

    private static string HtmlEncode(string value) => WebUtility.HtmlEncode(value ?? string.Empty);

    private static string RenderEstimatedTime(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return $"""
            <div class="alert alert-warning mb-0">
                <i class="bi bi-clock me-2"></i>Dự kiến hoạt động lại: <strong>{HtmlEncode(value)}</strong>
            </div>
            """;
    }
}

public static class MaintenanceModeMiddlewareExtensions
{
    public static IApplicationBuilder UseMaintenanceMode(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MaintenanceModeMiddleware>();
    }
}
