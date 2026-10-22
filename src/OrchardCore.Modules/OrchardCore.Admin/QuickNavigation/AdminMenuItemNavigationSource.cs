using System.Text.Json;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Navigation;

namespace OrchardCore.Admin.QuickNavigation;

public sealed class AdminMenuItemNavigationSource : QuickNavigationSource
{
    private readonly INavigationManager _navigationManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Dictionary<string, QuickNavigationResult> _entries = new(StringComparer.Ordinal);

    public AdminMenuItemNavigationSource(INavigationManager navigationManager, IHttpContextAccessor httpContextAccessor)
    {
        _navigationManager = navigationManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public override string Name => nameof(AdminMenuItemNavigationSource);

    public override async ValueTask<IEnumerable<string>> GetEntryIdsAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("Admin menu navigation requires an active HTTP request.");

        // Menu providers can depend on the request. Never share this authorized menu across users.
        var menu = await _navigationManager.BuildMenuAsync(NavigationConstants.AdminId, await httpContext.GetActionContextAsync());
        _entries.Clear();
        AddEntries(menu, [], []);

        return _entries.Keys;
    }

    public override ValueTask<QuickNavigationResult> DisplayAsync(string entryId)
        => ValueTask.FromResult(_entries.GetValueOrDefault(entryId));

    private void AddEntries(IEnumerable<MenuItem> items, string[] names, string[] path)
    {
        foreach (var item in items.OrderBy(item => item.Position, FlatPositionComparer.Instance))
        {
            var title = item.Text?.Value;
            var itemNames = string.IsNullOrWhiteSpace(title) ? names : [.. names, item.Text.Name];

            if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrEmpty(item.Href) && item.Href != "#")
            {
                var id = JsonSerializer.Serialize(new { Names = itemNames, item.Href });
                _entries.TryAdd(id, new QuickNavigationResult
                {
                    Title = title,
                    Path = path,
                    Href = item.Href,
                    Target = item.Target,
                });
            }

            AddEntries(item.Items, itemNames, string.IsNullOrWhiteSpace(title) ? path : [.. path, title]);
        }
    }
}
