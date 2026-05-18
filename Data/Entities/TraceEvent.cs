namespace WebActionResults.Data.Entities;

public class TraceEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..12].ToUpper();
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;

    // Actor
    public string ActorType { get; set; } = "Anonymous"; // "User" | "Anonymous"
    public string? ActorName { get; set; }
    public string? ActorEmail { get; set; }
    public string? ActorRole { get; set; }
    public string? VisitorId { get; set; }   // for anonymous
    public string? SessionId { get; set; }

    // Network
    public string Ip { get; set; } = string.Empty;
    public string IpMasked { get; set; } = string.Empty;  // xxx.xxx.*.*
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Isp { get; set; }
    public bool IsNewIp { get; set; }

    // Device
    public string? Browser { get; set; }
    public string? Os { get; set; }
    public string? Device { get; set; }    // Desktop | Mobile | Tablet
    public bool IsNewDevice { get; set; }

    // Action
    public string Action { get; set; } = string.Empty;   // human-readable VN
    public string? ActionType { get; set; }               // LOGIN | LOGOUT | VIEW | EDIT | DELETE | EXPORT | ROLE_CHANGE | etc.
    public string? Target { get; set; }                   // đối tượng bị ảnh hưởng
    public string PagePath { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    public int StatusCode { get; set; } = 200;
    public long DurationMs { get; set; }
    public string? RequestId { get; set; }

    // Risk
    public int RiskScore { get; set; }
    public RiskLevel RiskLevel { get; set; } = RiskLevel.Low;
    public List<string> RiskReasons { get; set; } = new();

    // Data changes
    public DataChange? DataChanges { get; set; }

    // Journey (for anonymous visitors)
    public List<string>? Journey { get; set; }

    // Extra tags for filtering
    public List<string> Tags { get; set; } = new();
}

public class DataChange
{
    public object? Before { get; set; }
    public object? After { get; set; }
    public string? Field { get; set; }
    public string? Description { get; set; }
}

public enum RiskLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}
