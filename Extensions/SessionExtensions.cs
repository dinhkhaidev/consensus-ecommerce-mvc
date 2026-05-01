using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace WebActionResults.Extensions;

public static class SessionExtensions
{
    public static void SetObjectAsJson(this ISession session, string key, object value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static T? GetObjectFromJson<T>(this ISession session, string key)
    {
        var data = session.GetString(key);
        return string.IsNullOrWhiteSpace(data) ? default : JsonSerializer.Deserialize<T>(data);
    }
}