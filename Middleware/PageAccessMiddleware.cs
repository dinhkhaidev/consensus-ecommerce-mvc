using System.Net;
using System.Globalization;
using WebActionResults.Data.Services;

namespace WebActionResults.Middleware;

public class PageAccessMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly PageAccessRule[] Rules =
    {
        new("PageAboutEnabled", "Gi\u1edbi thi\u1ec7u", new[] { "/Home/About" }),
        new("PageContactEnabled", "Li\u00ean h\u1ec7", new[] { "/Home/Contact" }),
        new("PagePoliciesEnabled", "Ch\u00ednh s\u00e1ch", new[] { "/Home/Privacy", "/Home/Terms", "/Home/ShippingPolicy" }),
        new("PageHomeEnabled", "Trang ch\u1ee7", new[] { "/", "/Home", "/Home/Index" }, ExactOnly: true),
        new("PageShopEnabled", "C\u1eeda h\u00e0ng", new[] { "/Product" }),
        new("PageCategoriesEnabled", "Danh m\u1ee5c", new[] { "/Category" }),
        new("PageRoom3DEnabled", "Ph\u00f2ng 3D", new[] { "/Room3D" }),
        new("PageAiStylistEnabled", "T\u01b0 v\u1ea5n AI", new[] { "/Ai" }),
        new("PageCartEnabled", "Gi\u1ecf h\u00e0ng", new[] { "/Cart" }),
        new("PageCheckoutEnabled", "Thanh to\u00e1n", new[] { "/Checkout", "/Payment" }),
        new("PageWishlistEnabled", "Y\u00eau th\u00edch", new[] { "/Wishlist" }),
        new("PageOrdersEnabled", "\u0110\u01a1n h\u00e0ng", new[] { "/Order" })
    };

    public PageAccessMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldBypass(context))
        {
            await _next(context);
            return;
        }

        var rule = FindRule(context.Request.Path);
        if (rule == null)
        {
            await _next(context);
            return;
        }

        var settingsService = context.RequestServices.GetRequiredService<IWebSettingsService>();
        var settings = await settingsService.GetAllSettingsAsync();
        var settingValue = settings.GetValueOrDefault(rule.SettingKey, "true");
        var isEnabled = !string.Equals(settingValue, "false", StringComparison.OrdinalIgnoreCase);
        var opensAt = GetOpenAt(settings, OpenAtSettingKey(rule));

        if (isEnabled || (opensAt.HasValue && opensAt.Value <= DateTimeOffset.Now))
        {
            await _next(context);
            return;
        }

        await RenderClosedPageAsync(context, rule, settings);
    }

    private static bool ShouldBypass(HttpContext context)
    {
        var path = context.Request.Path;
        var role = context.Session.GetString("USER_ROLE");
        var isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

        return isAdmin
            || path.StartsWithSegments("/Admin", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/Account", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/Language", StringComparison.OrdinalIgnoreCase)
            || IsPaymentCallbackPath(path)
            || path.StartsWithSegments("/Fulfillment", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPaymentCallbackPath(PathString path)
    {
        return path.StartsWithSegments("/Payment/Callback", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/Payment/VNPayIPN", StringComparison.OrdinalIgnoreCase);
    }

    private static PageAccessRule? FindRule(PathString requestPath)
    {
        var path = requestPath.Value ?? "/";

        foreach (var rule in Rules)
        {
            foreach (var prefix in rule.Paths)
            {
                if (rule.ExactOnly)
                {
                    if (string.Equals(path.TrimEnd('/'), prefix.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                        return rule;
                    continue;
                }

                if (requestPath.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase))
                    return rule;
            }
        }

        return null;
    }

    private static async Task RenderClosedPageAsync(HttpContext context, PageAccessRule rule, Dictionary<string, string> settings)
    {
        var pageName = rule.Label;
        var eyebrow = SettingOrDefault(settings, "ClosedPageEyebrow", "Khu v\u1ef1c \u0111\u1eb7c bi\u1ec7t");
        var title = SettingOrDefault(settings, "ClosedPageTitle", "Khu v\u1ef1c n\u00e0y \u0111ang t\u1ea1m kh\u00f3a");
        var message = SettingOrDefault(settings, "ClosedPageMessage", "Qu\u1ea3n tr\u1ecb vi\u00ean \u0111ang gi\u1eef t\u00ednh n\u0103ng n\u00e0y \u1edf ch\u1ebf \u0111\u1ed9 b\u00ed m\u1eadt. Vui l\u00f2ng quay l\u1ea1i sau.");
        var buttonText = SettingOrDefault(settings, "ClosedPageButtonText", "V\u1ec1 trang ch\u1ee7");
        var backgroundImageUrl = SettingOrDefault(settings, "ClosedPageBackgroundImageUrl", "/assets/images/sontungmtpmv.png");
        var opensAt = GetOpenAt(settings, OpenAtSettingKey(rule));
        var opensAtIso = opensAt?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty;
        var opensAtDisplay = opensAt?.ToString("HH:mm, dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN")) ?? "Ch\u01b0a h\u1eb9n gi\u1edd m\u1edf";
        var countdownClass = opensAt.HasValue ? string.Empty : " is-empty";

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "text/html; charset=utf-8";
        context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        context.Response.Headers.Pragma = "no-cache";

        await context.Response.WriteAsync($$"""
<!doctype html>
<html lang="vi">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta name="robots" content="noindex,nofollow">
    <title>{{HtmlEncode(title)}}</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/css/bootstrap.min.css" rel="stylesheet">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.13.1/font/bootstrap-icons.css">
    <style>
        :root {
            --ink: #251f1a;
            --muted: #75685d;
            --brand: #d47f31;
            --brand-dark: #8d4d1f;
            --line: rgba(141, 77, 31, .18);
            --locked-mask-image: url('{{HtmlEncode(backgroundImageUrl)}}');
        }

        * { box-sizing: border-box; }

        body {
            min-height: 100vh;
            margin: 0;
            color: var(--ink);
            font-family: Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
            background:
                radial-gradient(circle at 16% 18%, rgba(212, 127, 49, .2), transparent 28%),
                radial-gradient(circle at 82% 16%, rgba(250, 204, 21, .12), transparent 26%),
                radial-gradient(circle at 70% 84%, rgba(120, 53, 15, .24), transparent 30%),
                linear-gradient(135deg, #120d0a 0%, #25170f 46%, #100d0c 100%);
            overflow-x: hidden;
        }

        body::before {
            content: "";
            position: fixed;
            inset: 0;
            pointer-events: none;
            background-image:
                linear-gradient(rgba(255, 231, 196, .035) 1px, transparent 1px),
                linear-gradient(90deg, rgba(255, 231, 196, .035) 1px, transparent 1px);
            background-size: 34px 34px;
            mask-image: linear-gradient(to bottom, rgba(0, 0, 0, .85), transparent 90%);
            z-index: 1;
        }

        .mask-wall {
            position: fixed;
            inset: 0;
            z-index: 0;
            display: grid;
            grid-template-columns: repeat(6, minmax(108px, 1fr));
            gap: clamp(14px, 2.5vw, 34px);
            padding: clamp(18px, 4vw, 58px);
            pointer-events: none;
            overflow: hidden;
        }

        .mask-tile {
            min-height: 120px;
            aspect-ratio: 1 / 1.08;
            border-radius: 22px;
            background:
                linear-gradient(180deg, rgba(20, 12, 8, .02), rgba(20, 12, 8, .16)),
                var(--locked-mask-image) center / cover no-repeat;
            border: 1px solid rgba(255, 226, 184, .18);
            box-shadow: 0 18px 52px rgba(0, 0, 0, .42);
            filter: sepia(.08) saturate(1.08) contrast(1.24) brightness(.92);
            opacity: var(--o, .58);
            transform: translate3d(var(--x, 0), var(--y, 0), 0) rotate(var(--r, 0deg)) scale(var(--s, 1));
        }

        .mask-wall::after {
            content: "";
            position: absolute;
            inset: -12%;
            background:
                radial-gradient(circle at 50% 45%, rgba(15, 10, 8, .08) 0 18%, rgba(15, 10, 8, .22) 42%, rgba(15, 10, 8, .72) 82%),
                linear-gradient(115deg, rgba(255, 196, 112, .08), transparent 35%, rgba(0, 0, 0, .18));
            backdrop-filter: blur(.4px);
        }

        .closed-shell {
            position: relative;
            z-index: 2;
            min-height: 100vh;
            display: grid;
            place-items: center;
            padding: 28px;
        }

        .closed-panel {
            position: relative;
            width: min(760px, 100%);
            overflow: hidden;
            background:
                linear-gradient(145deg, rgba(255, 251, 244, .92), rgba(247, 228, 209, .82));
            border: 1px solid rgba(255, 235, 205, .74);
            border-radius: 28px;
            box-shadow: 0 34px 100px rgba(0, 0, 0, .42), 0 0 0 1px rgba(255, 255, 255, .22) inset;
            backdrop-filter: blur(18px);
        }

        .closed-panel::before {
            content: "";
            position: absolute;
            inset: 0 0 auto 0;
            height: 8px;
            background: linear-gradient(90deg, #d47f31, #ffd08a, #1f2933, #d47f31);
            background-size: 240% 100%;
            animation: shimmer 5s linear infinite;
        }

        .closed-content {
            position: relative;
            z-index: 1;
            padding: clamp(28px, 6vw, 58px);
        }

        .closed-icon {
            width: 88px;
            height: 88px;
            display: grid;
            place-items: center;
            margin: 0 auto 24px;
            color: #fff;
            background: linear-gradient(145deg, #d47f31, #8d4d1f);
            border-radius: 28px;
            box-shadow: 0 18px 42px rgba(212, 127, 49, .38);
            transform: rotate(-3deg);
            animation: floaty 3.2s ease-in-out infinite;
        }

        .closed-kicker {
            display: inline-flex;
            align-items: center;
            gap: 8px;
            padding: 9px 14px;
            color: var(--brand-dark);
            background: rgba(212, 127, 49, .12);
            border: 1px solid rgba(212, 127, 49, .22);
            border-radius: 999px;
            font-size: .78rem;
            font-weight: 800;
            letter-spacing: .08em;
            text-transform: uppercase;
        }

        h1 {
            max-width: 620px;
            margin: 18px auto 14px;
            font-size: clamp(2rem, 5vw, 4rem);
            line-height: .98;
            font-weight: 900;
            letter-spacing: -.04em;
        }

        .closed-message {
            max-width: 560px;
            margin: 0 auto;
            color: var(--muted);
            font-size: 1.06rem;
            line-height: 1.7;
        }

        .closed-countdown {
            display: grid;
            grid-template-columns: minmax(110px, .8fr) minmax(0, 1.7fr);
            gap: 18px;
            align-items: center;
            max-width: 600px;
            margin: 26px auto 0;
            padding: 16px;
            text-align: left;
            background: rgba(37, 31, 26, .92);
            border: 1px solid rgba(255, 213, 154, .28);
            border-radius: 24px;
            box-shadow: 0 18px 44px rgba(37, 31, 26, .22);
        }

        .clock-face {
            min-height: 118px;
            display: grid;
            place-items: center;
            align-content: center;
            gap: 8px;
            color: #ffe7bd;
            border-radius: 20px;
            background:
                radial-gradient(circle at 50% 45%, rgba(255, 226, 184, .18), transparent 38%),
                linear-gradient(145deg, rgba(255, 255, 255, .08), rgba(255, 255, 255, .02));
            border: 1px solid rgba(255, 226, 184, .24);
        }

        .clock-face i {
            font-size: 2.4rem;
            filter: drop-shadow(0 0 16px rgba(255, 196, 112, .42));
        }

        .clock-time {
            font-variant-numeric: tabular-nums;
            font-size: .9rem;
            font-weight: 900;
            letter-spacing: .08em;
        }

        .countdown-copy {
            color: rgba(255, 247, 232, .72);
            font-size: .86rem;
            line-height: 1.5;
        }

        .countdown-copy strong {
            display: block;
            margin-top: 3px;
            color: #fff1cf;
            font-size: 1rem;
        }

        .countdown-grid {
            display: grid;
            grid-template-columns: repeat(4, minmax(0, 1fr));
            gap: 8px;
            margin-top: 12px;
        }

        .countdown-unit {
            min-width: 0;
            padding: 10px 8px;
            text-align: center;
            color: #fff8ed;
            border-radius: 16px;
            background: rgba(255, 255, 255, .08);
            border: 1px solid rgba(255, 255, 255, .1);
        }

        .countdown-unit b {
            display: block;
            font-variant-numeric: tabular-nums;
            font-size: clamp(1.15rem, 3vw, 1.65rem);
            line-height: 1;
        }

        .countdown-unit span {
            display: block;
            margin-top: 5px;
            color: rgba(255, 247, 232, .58);
            font-size: .68rem;
            font-weight: 800;
            text-transform: uppercase;
            letter-spacing: .08em;
        }

        .closed-countdown.is-empty .countdown-grid {
            display: none;
        }

        .closed-meme-row {
            display: flex;
            flex-wrap: wrap;
            justify-content: center;
            gap: 14px;
            margin: 26px 0 30px;
        }

        .meme-icon {
            width: 48px;
            height: 48px;
            display: grid;
            place-items: center;
            color: #4a3a2c;
            background: rgba(255, 255, 255, .66);
            border: 1px solid var(--line);
            border-radius: 16px;
            font-size: 1.25rem;
            box-shadow: 0 12px 26px rgba(75, 49, 28, .1);
            transform: rotate(var(--tilt, 0deg));
            transition: transform .18s ease, background .18s ease;
        }

        .meme-icon:hover {
            background: rgba(255, 255, 255, .9);
            transform: translateY(-4px) rotate(var(--tilt, 0deg));
        }

        .closed-btn {
            display: inline-flex;
            align-items: center;
            gap: 10px;
            min-height: 48px;
            padding: 0 22px;
            color: #fff;
            text-decoration: none;
            font-weight: 800;
            border-radius: 999px;
            background: linear-gradient(135deg, #251f1a, #4c382a);
            box-shadow: 0 16px 32px rgba(37, 31, 26, .22);
            transition: transform .18s ease, box-shadow .18s ease;
        }

        .closed-btn:hover {
            color: #fff;
            transform: translateY(-2px);
            box-shadow: 0 22px 44px rgba(37, 31, 26, .28);
        }

        .spark {
            position: absolute;
            display: grid;
            place-items: center;
            color: var(--brand);
            background: rgba(255, 255, 255, .72);
            border: 1px solid rgba(255, 255, 255, .9);
            border-radius: 18px;
            box-shadow: 0 14px 34px rgba(75, 49, 28, .14);
            animation: floaty 4s ease-in-out infinite;
        }

        .spark.one { width: 54px; height: 54px; left: 26px; top: 86px; animation-delay: -.8s; }
        .spark.two { width: 46px; height: 46px; right: 34px; top: 54px; animation-delay: -1.8s; }
        .spark.three { width: 42px; height: 42px; right: 54px; bottom: 68px; animation-delay: -2.6s; }

        @keyframes floaty {
            0%, 100% { transform: translateY(0) rotate(-3deg); }
            50% { transform: translateY(-10px) rotate(3deg); }
        }

        @keyframes shimmer {
            to { background-position: 240% 0; }
        }

        @media (max-width: 576px) {
            .closed-shell { padding: 16px; }
            .mask-wall {
                grid-template-columns: repeat(3, minmax(88px, 1fr));
                gap: 12px;
                padding: 12px;
            }
            .mask-tile {
                min-height: 104px;
                border-radius: 18px;
            }
            .closed-countdown {
                grid-template-columns: 1fr;
                text-align: center;
                border-radius: 20px;
            }
            .countdown-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
            .spark { display: none; }
            .closed-panel { border-radius: 20px; }
            .closed-content { padding: 32px 22px; }
            .closed-icon { width: 76px; height: 76px; border-radius: 24px; }
        }
    </style>
</head>
<body>
    <div class="mask-wall" aria-hidden="true">
        <span class="mask-tile" style="--r:-7deg;--x:-8px;--y:4px;--o:.52;--s:.95"></span>
        <span class="mask-tile" style="--r:3deg;--y:22px;--o:.6"></span>
        <span class="mask-tile" style="--r:-2deg;--x:4px;--o:.54;--s:1.08"></span>
        <span class="mask-tile" style="--r:6deg;--y:12px;--o:.68"></span>
        <span class="mask-tile" style="--r:-4deg;--x:10px;--o:.58"></span>
        <span class="mask-tile" style="--r:8deg;--y:-8px;--o:.48;--s:.94"></span>
        <span class="mask-tile" style="--r:5deg;--x:-14px;--o:.64"></span>
        <span class="mask-tile" style="--r:-6deg;--y:10px;--o:.46;--s:.92"></span>
        <span class="mask-tile" style="--r:2deg;--x:6px;--o:.62"></span>
        <span class="mask-tile" style="--r:-3deg;--y:26px;--o:.5"></span>
        <span class="mask-tile" style="--r:7deg;--o:.7;--s:1.04"></span>
        <span class="mask-tile" style="--r:-8deg;--x:12px;--y:8px;--o:.52"></span>
        <span class="mask-tile" style="--r:-2deg;--x:-4px;--o:.6"></span>
        <span class="mask-tile" style="--r:6deg;--y:-10px;--o:.48;--s:.9"></span>
        <span class="mask-tile" style="--r:-5deg;--y:18px;--o:.66"></span>
        <span class="mask-tile" style="--r:4deg;--x:16px;--o:.54"></span>
        <span class="mask-tile" style="--r:-7deg;--y:4px;--o:.62;--s:1.05"></span>
        <span class="mask-tile" style="--r:5deg;--x:-10px;--o:.46"></span>
    </div>
    <main class="closed-shell">
        <section class="closed-panel text-center">
            <span class="spark one"><i class="bi bi-stars fs-4"></i></span>
            <span class="spark two"><i class="bi bi-key fs-5"></i></span>
            <span class="spark three"><i class="bi bi-magic fs-5"></i></span>

            <div class="closed-content">
                <div class="closed-icon"><i class="bi bi-lock-fill fs-1"></i></div>
                <div class="closed-kicker"><i class="bi bi-shield-lock"></i>{{HtmlEncode(eyebrow)}} / {{HtmlEncode(pageName)}}</div>
                <h1>{{HtmlEncode(title)}}</h1>
                <p class="closed-message">{{HtmlEncode(message)}}</p>
                <div class="closed-countdown{{countdownClass}}" data-unlock-at="{{HtmlEncode(opensAtIso)}}">
                    <div class="clock-face">
                        <i class="bi bi-clock-history"></i>
                        <span class="clock-time" data-clock-now>--:--:--</span>
                    </div>
                    <div class="countdown-copy">
                        <span>Thời gian mở dự kiến</span>
                        <strong data-unlock-label>{{HtmlEncode(opensAtDisplay)}}</strong>
                        <div class="countdown-grid" aria-label="Đồng hồ đếm ngược">
                            <div class="countdown-unit"><b data-count-days>--</b><span>ngày</span></div>
                            <div class="countdown-unit"><b data-count-hours>--</b><span>giờ</span></div>
                            <div class="countdown-unit"><b data-count-minutes>--</b><span>phút</span></div>
                            <div class="countdown-unit"><b data-count-seconds>--</b><span>giây</span></div>
                        </div>
                    </div>
                </div>
                <div class="closed-meme-row" aria-label="Trạng thái vui của trang đang khóa">
                    <span class="meme-icon" style="--tilt:-7deg" title="Cửa đang khóa"><i class="bi bi-door-closed"></i></span>
                    <span class="meme-icon" style="--tilt:5deg" title="Chìa khóa đang đi lạc"><i class="bi bi-key"></i></span>
                    <span class="meme-icon" style="--tilt:-3deg" title="Tính năng đang ngủ"><i class="bi bi-moon-stars"></i></span>
                    <span class="meme-icon" style="--tilt:6deg" title="Admin đang giữ bí mật"><i class="bi bi-incognito"></i></span>
                </div>
                <a class="closed-btn" href="/"><i class="bi bi-arrow-left-short fs-4"></i>{{HtmlEncode(buttonText)}}</a>
            </div>
        </section>
    </main>
    <script>
        (() => {
            const countdown = document.querySelector("[data-unlock-at]");
            const targetRaw = countdown?.dataset.unlockAt;
            const clockNow = document.querySelector("[data-clock-now]");

            const pad = value => String(value).padStart(2, "0");
            const setText = (selector, value) => {
                const element = document.querySelector(selector);
                if (element) element.textContent = value;
            };

            function tick() {
                const now = new Date();
                if (clockNow) {
                    clockNow.textContent = `${pad(now.getHours())}:${pad(now.getMinutes())}:${pad(now.getSeconds())}`;
                }

                if (!targetRaw) return;

                const target = new Date(targetRaw);
                const remaining = Math.max(0, target.getTime() - now.getTime());
                const totalSeconds = Math.floor(remaining / 1000);
                const days = Math.floor(totalSeconds / 86400);
                const hours = Math.floor((totalSeconds % 86400) / 3600);
                const minutes = Math.floor((totalSeconds % 3600) / 60);
                const seconds = totalSeconds % 60;

                setText("[data-count-days]", days);
                setText("[data-count-hours]", pad(hours));
                setText("[data-count-minutes]", pad(minutes));
                setText("[data-count-seconds]", pad(seconds));

                if (remaining <= 0) {
                    setTimeout(() => window.location.reload(), 650);
                }
            }

            tick();
            setInterval(tick, 1000);
        })();
    </script>
</body>
</html>
""");
    }

    private static string OpenAtSettingKey(PageAccessRule rule)
    {
        const string suffix = "Enabled";
        return rule.SettingKey.EndsWith(suffix, StringComparison.Ordinal)
            ? string.Concat(rule.SettingKey.AsSpan(0, rule.SettingKey.Length - suffix.Length), "OpenAt")
            : rule.SettingKey + "OpenAt";
    }

    private static DateTimeOffset? GetOpenAt(Dictionary<string, string> settings, string key)
    {
        if (!settings.TryGetValue(key, out var rawValue) || string.IsNullOrWhiteSpace(rawValue))
            return null;

        var raw = rawValue.Trim();

        if (DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var offsetValue))
            return offsetValue;

        if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var localValue))
            return new DateTimeOffset(localValue);

        return null;
    }

    private static string SettingOrDefault(Dictionary<string, string> settings, string key, string fallback)
    {
        return settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;
    }

    private static string HtmlEncode(string value) => WebUtility.HtmlEncode(value ?? string.Empty);

    private sealed record PageAccessRule(string SettingKey, string Label, string[] Paths, bool ExactOnly = false);
}

public static class PageAccessMiddlewareExtensions
{
    public static IApplicationBuilder UsePageAccessControl(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<PageAccessMiddleware>();
    }
}
