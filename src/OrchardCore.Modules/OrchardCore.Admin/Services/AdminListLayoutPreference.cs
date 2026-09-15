using System.Text;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Admin.Services;

/// <summary>
/// Remembers the layout a user picked for a list, in a cookie of their own, so the list opens the same way on
/// the next request. Only used when the site allows a user to choose, see
/// <see cref="AdminListOptions.AllowUserSelection"/>.
/// </summary>
/// <remarks>
/// One cookie holds every list, as <c>Contents:Table|Users:Grid</c>, which keeps a browser to a single cookie
/// however many lists the user visits. Values that are not plain names are dropped while reading, so a
/// hand-edited cookie cannot reach a template name.
/// </remarks>
public sealed class AdminListLayoutPreference
{
    /// <summary>
    /// The name of the cookie holding the layouts a user picked.
    /// </summary>
    public const string CookieName = "orchardcore_admin_list_layout";

    /// <summary>
    /// The query string key a list uses to ask for a layout, e.g. <c>?layout=Grid</c>.
    /// </summary>
    public const string QueryKey = "layout";

    // A cookie is shared by every list of the site, so it stays small on purpose.
    private const int MaxEntries = 50;

    private readonly IHttpContextAccessor _httpContextAccessor;

    private Dictionary<string, string> _entries;

    public AdminListLayoutPreference(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the layout the user picked for the given list, if any.
    /// </summary>
    public bool TryGet(string listName, out string layout)
    {
        layout = null;

        if (string.IsNullOrEmpty(listName))
        {
            return false;
        }

        return Entries.TryGetValue(listName, out layout) && !string.IsNullOrEmpty(layout);
    }

    /// <summary>
    /// Remembers the layout the user picked for the given list.
    /// </summary>
    public void Set(string listName, string layout)
    {
        var context = _httpContextAccessor.HttpContext;

        if (context is null || context.Response.HasStarted || !IsName(listName) || !IsName(layout))
        {
            return;
        }

        var entries = Entries;

        if (entries.TryGetValue(listName, out var current) && string.Equals(current, layout, StringComparison.Ordinal))
        {
            // Already what the cookie says, so the response does not need to carry it again.
            return;
        }

        if (entries.Count >= MaxEntries && !entries.ContainsKey(listName))
        {
            // The user visited more lists than the cookie holds: the oldest choice makes room.
            entries.Remove(entries.Keys.First());
        }

        entries[listName] = layout;

        context.Response.Cookies.Append(CookieName, Serialize(entries), new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Secure = context.Request.IsHttps,
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            Path = "/",
        });
    }

    private Dictionary<string, string> Entries
        => _entries ??= Parse(_httpContextAccessor.HttpContext?.Request.Cookies[CookieName]);

    private static Dictionary<string, string> Parse(string value)
    {
        var entries = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrEmpty(value))
        {
            return entries;
        }

        foreach (var entry in value.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separator = entry.IndexOf(':');

            if (separator <= 0 || separator == entry.Length - 1)
            {
                continue;
            }

            var listName = entry[..separator];
            var layout = entry[(separator + 1)..];

            if (IsName(listName) && IsName(layout) && entries.Count < MaxEntries)
            {
                entries[listName] = layout;
            }
        }

        return entries;
    }

    private static string Serialize(Dictionary<string, string> entries)
    {
        var builder = new StringBuilder();

        foreach (var entry in entries)
        {
            if (builder.Length > 0)
            {
                builder.Append('|');
            }

            builder.Append(entry.Key).Append(':').Append(entry.Value);
        }

        return builder.ToString();
    }

    // A list and a layout are plain names, e.g. Contents and Grid. Anything else is not one of ours.
    private static bool IsName(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length > 64)
        {
            return false;
        }

        foreach (var c in value)
        {
            if (!char.IsAsciiLetterOrDigit(c) && c != '.' && c != '_' && c != '-')
            {
                return false;
            }
        }

        return true;
    }
}
