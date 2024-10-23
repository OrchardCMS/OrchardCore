using System.Collections.Concurrent;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Navigation;

namespace OrchardCore.Settings;

public abstract class SettingsNavigationProvider : NamedNavigationProvider
{
    private static readonly ConcurrentDictionary<string, RouteValueDictionary> _groups = new(StringComparer.OrdinalIgnoreCase);

    protected static RouteValueDictionary GetRouteValues(string groupId)
    {
        if (!_groups.TryGetValue(groupId ?? "", out var group))
        {
            group = new RouteValueDictionary
            {
                { "action", "Index" },
                { "controller", "Admin" },
                { "area", "OrchardCore.Settings" },
                { "groupId", groupId ?? "" },
            };

            _groups.TryAdd(groupId, group);
        }

        return group;
    }

    public SettingsNavigationProvider()
        : base(NavigationConstants.SiteSettingsId)
    {
    }
}
