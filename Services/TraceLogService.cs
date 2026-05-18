using System.Text.Json;
using System.Text.Json.Serialization;
using WebActionResults.Data.Entities;

namespace WebActionResults.Services;

public class TraceLogService : ITraceLogService
{
    private readonly string _filePath;
    private readonly string _archiveDir;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private const int MaxRecords = 10_000;

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public TraceLogService(IWebHostEnvironment env)
    {
        var appData = Path.Combine(env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(appData);
        _filePath = Path.Combine(appData, "trace-logs.json");
        _archiveDir = Path.Combine(appData, "trace-archive");
        Directory.CreateDirectory(_archiveDir);

        // Seed file if missing
        if (!File.Exists(_filePath))
            File.WriteAllText(_filePath, "[]");
    }

    // ── Write ────────────────────────────────────────────────────────────────

    public async Task WriteEventAsync(TraceEvent evt)
    {
        await _lock.WaitAsync();
        try
        {
            var events = await ReadAllAsync();

            // Auto-rotate if over limit
            if (events.Count >= MaxRecords)
            {
                var archiveName = $"trace-logs-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
                var archivePath = Path.Combine(_archiveDir, archiveName);
                var archiveJson = JsonSerializer.Serialize(events.Take(MaxRecords / 2).ToList(), _jsonOpts);
                await File.WriteAllTextAsync(archivePath, archiveJson);
                events = events.Skip(MaxRecords / 2).ToList();
            }

            events.Insert(0, evt);   // newest first
            var json = JsonSerializer.Serialize(events, _jsonOpts);
            await File.WriteAllTextAsync(_filePath, json);
        }
        finally { _lock.Release(); }
    }

    // ── Read / Query ─────────────────────────────────────────────────────────

    public async Task<TracePagedResult> GetEventsAsync(TraceFilter filter)
    {
        await _lock.WaitAsync();
        try
        {
            var all = await ReadAllAsync();
            var query = all.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.ToLower();
                query = query.Where(e =>
                    (e.ActorName?.ToLower().Contains(s) ?? false) ||
                    (e.ActorEmail?.ToLower().Contains(s) ?? false) ||
                    e.Action.ToLower().Contains(s) ||
                    e.Ip.Contains(s) ||
                    (e.PagePath?.ToLower().Contains(s) ?? false) ||
                    (e.Target?.ToLower().Contains(s) ?? false));
            }

            if (!string.IsNullOrWhiteSpace(filter.ActionType))
                query = query.Where(e => e.ActionType == filter.ActionType);

            if (!string.IsNullOrWhiteSpace(filter.Ip))
                query = query.Where(e => e.Ip.Contains(filter.Ip));

            if (!string.IsNullOrWhiteSpace(filter.Country))
                query = query.Where(e => e.Country == filter.Country);

            if (!string.IsNullOrWhiteSpace(filter.RiskLevel) &&
                Enum.TryParse<RiskLevel>(filter.RiskLevel, out var rl))
                query = query.Where(e => e.RiskLevel == rl);

            if (!string.IsNullOrWhiteSpace(filter.ActorType))
                query = query.Where(e => e.ActorType == filter.ActorType);

            if (filter.DateFrom.HasValue)
                query = query.Where(e => e.Timestamp >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(e => e.Timestamp <= filter.DateTo.Value);

            var total = query.Count();
            var page = Math.Max(1, filter.Page);
            var pageSize = Math.Clamp(filter.PageSize, 5, 100);

            var events = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new TracePagedResult { Events = events, Total = total, Page = page, PageSize = pageSize };
        }
        finally { _lock.Release(); }
    }

    public async Task<TraceEvent?> GetEventByIdAsync(string id)
    {
        await _lock.WaitAsync();
        try
        {
            var all = await ReadAllAsync();
            return all.FirstOrDefault(e => e.Id == id);
        }
        finally { _lock.Release(); }
    }

    public async Task<TraceSummary> GetSummaryAsync()
    {
        await _lock.WaitAsync();
        try
        {
            var all = await ReadAllAsync();
            var now = DateTimeOffset.Now;
            var today = now.Date;
            var thirtyMin = now.AddMinutes(-30);
            var todayEvents = all.Where(e => e.Timestamp.Date == today).ToList();

            // Active users (distinct names in last 30min)
            var activeUsers = all
                .Where(e => e.Timestamp >= thirtyMin && e.ActorType == "User")
                .GroupBy(e => e.ActorName)
                .Count();

            // Failed logins today
            var failedLogins = todayEvents.Count(e => e.ActionType == "LOGIN_FAIL");

            // New IPs today
            var newIpsToday = todayEvents.Count(e => e.IsNewIp);

            // Critical events
            var criticalEvents = all.Count(e => e.RiskLevel >= RiskLevel.High);

            // Top country
            var topCountry = all
                .Where(e => e.Country != null)
                .GroupBy(e => e.Country!)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? "VN";

            // Hourly counts (today)
            var hourlyCounts = Enumerable.Range(0, 24)
                .Select(h => new HourlyCount
                {
                    Hour = h,
                    Count = todayEvents.Count(e => e.Timestamp.Hour == h)
                }).ToList();

            // Top IPs
            var topIps = all
                .GroupBy(e => e.Ip)
                .Select(g => new IpCount { Ip = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            // Risk distribution
            var riskDist = Enum.GetValues<RiskLevel>()
                .Select(rl => new RiskCount
                {
                    Level = rl.ToString(),
                    Count = all.Count(e => e.RiskLevel == rl)
                }).ToList();

            // Country distribution
            var countryDist = all
                .Where(e => e.Country != null)
                .GroupBy(e => e.Country!)
                .Select(g => new CountryCount { Country = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(6)
                .ToList();

            // Device distribution
            var deviceDist = all
                .Where(e => e.Device != null)
                .GroupBy(e => e.Device!)
                .Select(g => new DeviceCount { Device = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            // Recent active users
            var recentActiveUsers = all
                .Where(e => e.Timestamp >= thirtyMin && e.ActorType == "User" && e.ActorName != null)
                .GroupBy(e => e.ActorName)
                .Select(g => new ActiveUserItem
                {
                    Name = g.Key!,
                    LastSeen = g.Max(e => e.Timestamp).ToString("HH:mm"),
                    Ip = g.OrderByDescending(e => e.Timestamp).First().IpMasked
                })
                .Take(8)
                .ToList();

            // Alert cards
            var alerts = BuildAlerts(all);

            return new TraceSummary
            {
                ActiveUsers = activeUsers,
                FailedLogins = failedLogins,
                NewIpsToday = newIpsToday,
                CriticalEvents = criticalEvents,
                TotalRequests = all.Count,
                TopCountry = topCountry,
                HourlyCounts = hourlyCounts,
                TopIps = topIps,
                RiskDistribution = riskDist,
                CountryDistribution = countryDist,
                DeviceDistribution = deviceDist,
                RecentActiveUsers = recentActiveUsers,
                Alerts = alerts
            };
        }
        finally { _lock.Release(); }
    }

    private static List<AlertCard> BuildAlerts(List<TraceEvent> all)
    {
        var alerts = new List<AlertCard>();

        // Group failed logins
        var failedLogins = all
            .Where(e => e.ActionType == "LOGIN_FAIL")
            .OrderByDescending(e => e.Timestamp)
            .FirstOrDefault();
        var failCount = all.Count(e => e.ActionType == "LOGIN_FAIL");
        if (failCount >= 3 && failedLogins != null)
            alerts.Add(new AlertCard
            {
                Id = "alert-failed-login",
                Title = $"{failCount} lần đăng nhập thất bại",
                Description = $"Phát hiện {failCount} lần đăng nhập thất bại liên tiếp",
                Ip = failedLogins.IpMasked,
                RiskLevel = failCount >= 10 ? "Critical" : "High",
                Timestamp = failedLogins.Timestamp,
                EventId = failedLogins.Id
            });

        // New country access
        var foreignAccess = all
            .Where(e => e.IsNewIp && e.Country != null && e.Country != "VN")
            .OrderByDescending(e => e.Timestamp)
            .FirstOrDefault();
        if (foreignAccess != null)
            alerts.Add(new AlertCard
            {
                Id = "alert-foreign-ip",
                Title = $"Đăng nhập bất thường từ {foreignAccess.Country}",
                Description = $"Phát hiện truy cập từ quốc gia lạ: {foreignAccess.Country}",
                Ip = foreignAccess.IpMasked,
                RiskLevel = "High",
                Timestamp = foreignAccess.Timestamp,
                EventId = foreignAccess.Id
            });

        // Role changes
        var roleChange = all
            .Where(e => e.ActionType == "ROLE_CHANGE")
            .OrderByDescending(e => e.Timestamp)
            .FirstOrDefault();
        if (roleChange != null)
            alerts.Add(new AlertCard
            {
                Id = "alert-role-change",
                Title = "Quyền admin đã bị thay đổi",
                Description = roleChange.Action,
                Ip = roleChange.IpMasked,
                RiskLevel = "Critical",
                Timestamp = roleChange.Timestamp,
                EventId = roleChange.Id
            });

        // Data export
        var dataExport = all
            .Where(e => e.ActionType == "EXPORT")
            .OrderByDescending(e => e.Timestamp)
            .FirstOrDefault();
        if (dataExport != null)
            alerts.Add(new AlertCard
            {
                Id = "alert-export",
                Title = "Người dùng đã xuất dữ liệu lớn",
                Description = dataExport.Action,
                Ip = dataExport.IpMasked,
                RiskLevel = "High",
                Timestamp = dataExport.Timestamp,
                EventId = dataExport.Id
            });

        return alerts.OrderByDescending(a => a.Timestamp).Take(4).ToList();
    }

    public async Task<string> ExportJsonAsync()
    {
        await _lock.WaitAsync();
        try
        {
            return await File.ReadAllTextAsync(_filePath);
        }
        finally { _lock.Release(); }
    }

    public async Task ClearLogsAsync()
    {
        await _lock.WaitAsync();
        try
        {
            await File.WriteAllTextAsync(_filePath, "[]");
        }
        finally { _lock.Release(); }
    }

    // ── Internal ─────────────────────────────────────────────────────────────

    private async Task<List<TraceEvent>> ReadAllAsync()
    {
        if (!File.Exists(_filePath)) return new();
        var json = await File.ReadAllTextAsync(_filePath);
        if (string.IsNullOrWhiteSpace(json) || json == "[]") return new();
        try
        {
            return JsonSerializer.Deserialize<List<TraceEvent>>(json, _jsonOpts) ?? new();
        }
        catch { return new(); }
    }
}
