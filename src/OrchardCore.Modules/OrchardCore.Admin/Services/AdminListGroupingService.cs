using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.Admin.Services;

internal sealed class AdminListGroupingService : IAdminListGroupingService
{
    internal const string CookieName = "orchardcore_admin_list_grouping";
    private readonly IServiceProvider _services;
    private readonly IHttpContextAccessor _accessor;
    private readonly Dictionary<string, AdminListGroupingSelection> _selections = new(StringComparer.Ordinal);
    private Dictionary<string, string> _preferences;

    public AdminListGroupingService(IServiceProvider services, IHttpContextAccessor accessor)
    {
        _services = services;
        _accessor = accessor;
    }

    public AdminListGroupingSelection GetSelection(string listName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(listName);
        if (_selections.TryGetValue(listName, out var selection))
        {
            return selection;
        }

        var providers = _services.GetKeyedServices<IAdminListGroupingProvider>(new AdminListGroupingProviderKey(listName)).ToArray();
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { AdminListGroupingSelection.None };
        foreach (var provider in providers)
        {
            if (!IsName(provider.Name) || !names.Add(provider.Name))
            {
                throw new InvalidOperationException($"Invalid or duplicate grouping '{provider.Name}' for admin list '{listName}'.");
            }
        }

        var context = _accessor.HttpContext;
        var entries = _preferences ??= ReadPreferences(context?.Request.Cookies[CookieName]);
        var hasQuery = context?.Request.Query.ContainsKey(AdminListGroupingSelection.QueryKey) == true;
        var requested = hasQuery
            ? context.Request.Query[AdminListGroupingSelection.QueryKey].ToString()
            : entries.GetValueOrDefault(listName);
        var selected = providers.FirstOrDefault(provider => string.Equals(provider.Name, requested, StringComparison.OrdinalIgnoreCase));
        selection = new AdminListGroupingSelection { Providers = providers, Provider = selected };
        _selections.Add(listName, selection);

        if (hasQuery && providers.Length > 0 && IsName(listName) && !context.Response.HasStarted
            && entries.GetValueOrDefault(listName) != selection.Name)
        {
            if (entries.Count >= 30 && !entries.ContainsKey(listName))
            {
                entries.Remove(entries.Keys.First());
            }

            entries[listName] = selection.Name;
            context.Response.Cookies.Append(CookieName, string.Join('|', entries.Select(entry => $"{entry.Key}:{entry.Value}")), new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = context.Request.IsHttps,
                Path = context.Request.PathBase.HasValue ? context.Request.PathBase.Value : "/",
                Expires = DateTimeOffset.UtcNow.AddYears(1),
            });
        }

        return selection;
    }

    public string GetUrl(string listName, string grouping)
    {
        var selection = GetSelection(listName);
        if (grouping != AdminListGroupingSelection.None && !selection.Providers.Any(provider => provider.Name == grouping))
        {
            throw new ArgumentException("The grouping is not registered for this list.", nameof(grouping));
        }

        var request = _accessor.HttpContext.Request;
        var query = request.Query.Where(pair =>
            !string.Equals(pair.Key, AdminListGroupingSelection.QueryKey, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(pair.Key, "page", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(pair.Key, "pagenum", StringComparison.OrdinalIgnoreCase))
            .Append(new KeyValuePair<string, StringValues>(AdminListGroupingSelection.QueryKey, grouping));
        return request.PathBase + request.Path + QueryString.Create(query);
    }

    private static Dictionary<string, string> ReadPreferences(string cookie)
    {
        var entries = new Dictionary<string, string>(StringComparer.Ordinal);
        if (cookie is null || cookie.Length > 4096)
        {
            return entries;
        }

        foreach (var entry in cookie.Split('|').Take(30))
        {
            var pair = entry.Split(':');
            if (pair.Length == 2 && IsName(pair[0]) && IsName(pair[1]))
            {
                entries[pair[0]] = pair[1];
            }
        }

        return entries;
    }

    private static bool IsName(string value)
        => !string.IsNullOrEmpty(value) && value.Length <= 64
            && value.All(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '_' or '-');
}
