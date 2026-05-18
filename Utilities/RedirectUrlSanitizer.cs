using System.Text;

namespace WebActionResults.Utilities;

public static class RedirectUrlSanitizer
{
    public static string EscapeHeaderValue(string url)
    {
        if (string.IsNullOrEmpty(url))
            return url;

        var builder = new StringBuilder(url.Length);
        foreach (var character in url)
        {
            if (character > 0x20 && character < 0x7f)
            {
                builder.Append(character);
                continue;
            }

            builder.Append(Uri.EscapeDataString(character.ToString()));
        }

        return builder.ToString();
    }
}
