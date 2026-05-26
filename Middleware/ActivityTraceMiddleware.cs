using System.Diagnostics;
using WebActionResults.Data.Entities;
using WebActionResults.Services;

namespace WebActionResults.Middleware;

public class ActivityTraceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ActivityTraceMiddleware> _logger;

    private static readonly string[] _skipPrefixes =
    {
        "/css/", "/js/", "/lib/", "/fonts/", "/images/", "/assets/",
        "/.well-known/", "/favicon", "/robots.txt", "/sitemap",
        "/Admin/AdminActivity",
        "/Cart/", "/Wishlist/GetCount", "/Home/GetCart",
        "/api/", "/_blazor", "/_framework",
        "/Account/AccessDenied"
    };

    private static readonly string[] _skipExtensions =
    {
        ".css", ".js", ".map", ".png", ".jpg", ".jpeg", ".gif", ".svg",
        ".ico", ".woff", ".woff2", ".ttf", ".eot", ".webp", ".avif",
        ".pdf", ".gltf", ".glb", ".fbx", ".bin"
    };

    public ActivityTraceMiddleware(RequestDelegate next, ILogger<ActivityTraceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "/";
        if (ShouldSkip(path, context.Request.Method))
        {
            await _next(context);
            return;
        }

        // ── Capture session data BEFORE the response completes ──
        string? actorName = null, actorEmail = null, actorRole = null;
        string? sessionId = null, visitorId = null;
        try
        {
            sessionId = context.Session?.Id;
            actorName = context.Session?.GetString("USER_NAME");
            actorEmail = context.Session?.GetString("USER_EMAIL");
            actorRole = context.Session?.GetString("USER_ROLE");
        }
        catch { /* Session not available for this request */ }

        visitorId = GetOrCreateVisitorId(context);
        var ip = GetClientIp(context);
        var ua = context.Request.Headers.UserAgent.ToString();
        var method = context.Request.Method;
        var requestId = context.TraceIdentifier;

        var sw = Stopwatch.StartNew();
        await _next(context);
        sw.Stop();

        var statusCode = context.Response.StatusCode;

        // Skip redirects (they'll be followed by the actual request)
        if (statusCode is >= 300 and < 400) return;

        var durationMs = sw.ElapsedMilliseconds;

        // ── Fire-and-forget log (all data already captured) ──
        var traceService = context.RequestServices.GetService<ITraceLogService>();
        if (traceService == null) return;

        _ = Task.Run(async () =>
        {
            try
            {
                var isAuthenticated = !string.IsNullOrEmpty(actorName);
                var (browser, os, device) = ParseUserAgent(ua);
                var (actionType, actionLabel, target, tags) = DetermineAction(path, method, statusCode, actorName, actorRole);
                var (riskScore, riskReasons) = CalculateRisk(actionType, path, statusCode, actorRole, ua, ip);

                var evt = new TraceEvent
                {
                    Timestamp = DateTimeOffset.Now,
                    ActorType = isAuthenticated ? "User" : "Anonymous",
                    ActorName = actorName,
                    ActorEmail = actorEmail,
                    ActorRole = actorRole,
                    VisitorId = isAuthenticated ? null : visitorId,
                    SessionId = sessionId ?? "unknown",
                    Ip = ip,
                    IpMasked = MaskIp(ip),
                    Country = "VN",
                    City = "Việt Nam",
                    Browser = browser,
                    Os = os,
                    Device = device,
                    Action = actionLabel,
                    ActionType = actionType,
                    Target = target,
                    PagePath = path,
                    Method = method,
                    StatusCode = statusCode,
                    DurationMs = durationMs,
                    RequestId = requestId,
                    RiskScore = riskScore,
                    RiskLevel = ScoreToLevel(riskScore),
                    RiskReasons = riskReasons,
                    Tags = tags
                };

                await traceService.WriteEventAsync(evt);
            }
            catch (Exception ex) { _logger.LogWarning(ex, "ActivityTrace: failed to record event"); }
        });
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static bool ShouldSkip(string path, string method)
    {
        foreach (var prefix in _skipPrefixes)
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return true;

        foreach (var ext in _skipExtensions)
            if (path.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                return true;

        return false;
    }

    private static string GetClientIp(HttpContext context)
    {
        var forwarded = context.Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrEmpty(forwarded))
            return forwarded.Split(',')[0].Trim();

        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        // Map IPv6 loopback to IPv4 for readability
        if (ip == "::1") ip = "127.0.0.1";
        return ip;
    }

    private static string GetOrCreateVisitorId(HttpContext context)
    {
        const string cookieName = "_vid";
        if (context.Request.Cookies.TryGetValue(cookieName, out var vid) && !string.IsNullOrEmpty(vid))
            return vid;

        var newVid = "A" + Guid.NewGuid().ToString("N")[..6].ToUpper();
        try
        {
            context.Response.Cookies.Append(cookieName, newVid, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            });
        }
        catch { /* Response may already started */ }
        return newVid;
    }

    private static string MaskIp(string ip)
    {
        if (ip == "127.0.0.1" || ip == "::1") return "localhost";
        if (ip.Contains(':')) return ip[..Math.Min(ip.Length, 8)] + "***";
        var parts = ip.Split('.');
        if (parts.Length == 4) return $"{parts[0]}.{parts[1]}.*.*";
        return ip;
    }

    private static (string browser, string os, string device) ParseUserAgent(string ua)
    {
        if (string.IsNullOrEmpty(ua)) return ("Unknown", "Unknown", "Desktop");

        string browser = "Other";
        if (ua.Contains("Edg/")) browser = "Edge";
        else if (ua.Contains("Chrome/") && !ua.Contains("Chromium")) browser = "Chrome";
        else if (ua.Contains("Firefox/")) browser = "Firefox";
        else if (ua.Contains("Safari/") && !ua.Contains("Chrome")) browser = "Safari";
        else if (ua.Contains("Opera") || ua.Contains("OPR/")) browser = "Opera";

        string os = "Other";
        if (ua.Contains("Windows NT")) os = "Windows";
        else if (ua.Contains("Mac OS X")) os = "macOS";
        else if (ua.Contains("Android")) os = "Android";
        else if (ua.Contains("iPhone") || ua.Contains("iPad")) os = "iOS";
        else if (ua.Contains("Linux")) os = "Linux";

        string device = "Desktop";
        if (ua.Contains("Mobile") || ua.Contains("iPhone")) device = "Mobile";
        else if (ua.Contains("Tablet") || ua.Contains("iPad")) device = "Tablet";

        return (browser, os, device);
    }

    private static (string actionType, string label, string? target, List<string> tags) DetermineAction(
        string path, string method, int statusCode, string? actorName, string? actorRole)
    {
        var tags = new List<string>();
        var lPath = path.ToLower();

        if (lPath.StartsWith("/admin")) tags.Add("admin");
        if (method == "POST") tags.Add("write");

        // Login/Logout
        if (lPath.Contains("/account/login") && method == "POST")
        {
            if (statusCode is >= 200 and < 400)
                return ("LOGIN", actorName != null ? $"{actorName} đã đăng nhập" : "Đăng nhập thành công", null, tags);
            return ("LOGIN_FAIL", "Đăng nhập thất bại", null, [..tags, "security"]);
        }
        if (lPath.Contains("/account/logout"))
            return ("LOGOUT", $"{actorName ?? "Người dùng"} đã đăng xuất", null, tags);

        // Admin role change
        if (lPath.Contains("/adminaccount") && method == "POST" && lPath.Contains("role"))
            return ("ROLE_CHANGE", $"{actorName ?? "Admin"} đã thay đổi vai trò người dùng", "Tài khoản", [..tags, "security"]);

        // Export
        if (lPath.Contains("export") || lPath.Contains("download"))
            return ("EXPORT", $"{actorName ?? "Người dùng"} đã xuất dữ liệu", "Dữ liệu", [..tags, "data"]);

        // Order actions
        if (lPath.Contains("/adminorder") && method == "POST")
            return ("ORDER_UPDATE", $"{actorName ?? "Admin"} đã cập nhật đơn hàng", "Đơn hàng", tags);

        // Product actions
        if (lPath.Contains("/adminproduct") && method == "POST")
            return ("PRODUCT_UPDATE", $"{actorName ?? "Admin"} đã chỉnh sửa sản phẩm", "Sản phẩm", tags);

        // Page views
        var pageName = GetPageName(lPath);
        if (actorName != null)
            return ("VIEW", $"{actorName} đã xem trang {pageName}", pageName, tags);

        return ("VIEW_ANON", $"Khách truy cập ẩn danh đã xem trang {pageName}", pageName, [..tags, "anonymous"]);
    }

    private static string GetPageName(string path)
    {
        if (path == "/" || path.Contains("/home/index") || path == "/home") return "Trang chủ";
        if (path.Contains("/product")) return "Sản phẩm";
        if (path.Contains("/category")) return "Danh mục";
        if (path.Contains("/cart")) return "Giỏ hàng";
        if (path.Contains("/checkout")) return "Thanh toán";
        if (path.Contains("/order")) return "Đơn hàng";
        if (path.Contains("/wishlist")) return "Yêu thích";
        if (path.Contains("/admin/dashboard")) return "Dashboard Admin";
        if (path.Contains("/admin/adminactivity")) return "Nhật ký hoạt động";
        if (path.Contains("/admin")) return "Khu vực Admin";
        if (path.Contains("/account/login")) return "Đăng nhập";
        if (path.Contains("/account/register")) return "Đăng ký";
        if (path.Contains("/home/about")) return "Giới thiệu";
        if (path.Contains("/home/contact")) return "Liên hệ";
        if (path.Contains("/home/privacy")) return "Chính sách bảo mật";
        if (path.Contains("/pricing") || path.Contains("/banggia")) return "Bảng giá";
        return path.Split('/').LastOrDefault(s => !string.IsNullOrEmpty(s)) ?? "Trang không xác định";
    }

    private static (int score, List<string> reasons) CalculateRisk(
        string actionType, string path, int statusCode, string? role,
        string ua, string ip)
    {
        var score = 0;
        var reasons = new List<string>();

        if (actionType == "LOGIN_FAIL")      { score += 25; reasons.Add("Đăng nhập thất bại"); }
        if (actionType == "ROLE_CHANGE")     { score += 30; reasons.Add("Thay đổi quyền hạn"); }
        if (actionType == "EXPORT")          { score += 20; reasons.Add("Xuất dữ liệu"); }
        if (path.ToLower().StartsWith("/admin")) { score += 10; reasons.Add("Truy cập khu vực Admin"); }
        if (statusCode >= 400)               { score += 10; reasons.Add("Yêu cầu bị từ chối"); }

        var hour = DateTime.Now.Hour;
        if (hour is < 7 or > 22)            { score += 15; reasons.Add("Truy cập ngoài giờ bình thường"); }

        return (Math.Min(score, 100), reasons);
    }

    private static RiskLevel ScoreToLevel(int score) => score switch
    {
        >= 70 => RiskLevel.Critical,
        >= 45 => RiskLevel.High,
        >= 20 => RiskLevel.Medium,
        _     => RiskLevel.Low
    };
}

public static class ActivityTraceMiddlewareExtensions
{
    public static IApplicationBuilder UseActivityTrace(this IApplicationBuilder builder)
        => builder.UseMiddleware<ActivityTraceMiddleware>();
}
