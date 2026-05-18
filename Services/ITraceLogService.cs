using WebActionResults.Data.Entities;

namespace WebActionResults.Services;

public interface ITraceLogService
{
    Task WriteEventAsync(TraceEvent evt);
    Task<TracePagedResult> GetEventsAsync(TraceFilter filter);
    Task<TraceSummary> GetSummaryAsync();
    Task<TraceEvent?> GetEventByIdAsync(string id);
    Task<string> ExportJsonAsync();
    Task ClearLogsAsync();
}

public class TraceFilter
{
    public string? Search { get; set; }
    public string? ActionType { get; set; }
    public string? Ip { get; set; }
    public string? Country { get; set; }
    public string? RiskLevel { get; set; }
    public string? ActorType { get; set; }
    public DateTimeOffset? DateFrom { get; set; }
    public DateTimeOffset? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class TracePagedResult
{
    public List<TraceEvent> Events { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
}

public class TraceSummary
{
    public int ActiveUsers { get; set; }
    public int FailedLogins { get; set; }
    public int NewIpsToday { get; set; }
    public int CriticalEvents { get; set; }
    public long TotalRequests { get; set; }
    public string TopCountry { get; set; } = "VN";
    public List<AlertCard> Alerts { get; set; } = new();
    public List<HourlyCount> HourlyCounts { get; set; } = new();
    public List<IpCount> TopIps { get; set; } = new();
    public List<RiskCount> RiskDistribution { get; set; } = new();
    public List<CountryCount> CountryDistribution { get; set; } = new();
    public List<DeviceCount> DeviceDistribution { get; set; } = new();
    public List<ActiveUserItem> RecentActiveUsers { get; set; } = new();
}

public class AlertCard
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = "Medium";
    public DateTimeOffset Timestamp { get; set; }
    public string EventId { get; set; } = string.Empty;
}

public class HourlyCount { public int Hour { get; set; } public int Count { get; set; } }
public class IpCount { public string Ip { get; set; } = string.Empty; public int Count { get; set; } }
public class RiskCount { public string Level { get; set; } = string.Empty; public int Count { get; set; } }
public class CountryCount { public string Country { get; set; } = string.Empty; public int Count { get; set; } }
public class DeviceCount { public string Device { get; set; } = string.Empty; public int Count { get; set; } }
public class ActiveUserItem { public string Name { get; set; } = string.Empty; public string LastSeen { get; set; } = string.Empty; public string Ip { get; set; } = string.Empty; }
