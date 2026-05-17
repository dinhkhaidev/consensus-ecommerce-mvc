using System.Security.Claims;
using WebActionResults.Data.Services;

namespace WebActionResults.Middleware;

public class SessionAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public SessionAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, Data.Services.IAuthenticationService authService)
    {
        // Skip authentication for static files
        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (path.StartsWith("/css") || path.StartsWith("/js") || path.StartsWith("/lib") || path.StartsWith("/images"))
        {
            await _next(context);
            return;
        }

        var userId = await authService.GetCurrentUserIdAsync();
        if (userId.HasValue)
        {
            var user = await authService.GetAccountByIdAsync(userId.Value);
            if (user != null)
            {
                var role = user.Role ?? "Customer";
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName ?? ""),
                    new Claim(ClaimTypes.Email, user.Email ?? ""),
                    new Claim("FullName", user.FullName ?? ""),
                    new Claim(ClaimTypes.Role, role)
                };

                var identity = new ClaimsIdentity(claims, "Session");
                context.User = new ClaimsPrincipal(identity);
            }
        }

        await _next(context);
    }
}

public static class SessionAuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseSessionAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SessionAuthenticationMiddleware>();
    }
}
