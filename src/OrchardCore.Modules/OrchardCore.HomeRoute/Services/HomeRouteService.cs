using System.Globalization;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Settings;

namespace OrchardCore.HomeRoute.Services;

internal sealed class HomeRouteService : IHomeRouteService
{
    private readonly ISiteService _siteService;

    public HomeRouteService(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public async Task<bool> UpdateAsync(Action<RouteValueDictionary> update)
    {
        ArgumentNullException.ThrowIfNull(update);

        var site = await _siteService.LoadSiteSettingsAsync();
        var current = site.HomeRoute;
        var route = current is null ? new RouteValueDictionary() : new RouteValueDictionary(current);
        update(route);

        if (route.Count == (current?.Count ?? 0) && route.All(entry =>
            current.TryGetValue(entry.Key, out var value) &&
            string.Equals(Convert.ToString(value, CultureInfo.InvariantCulture),
                Convert.ToString(entry.Value, CultureInfo.InvariantCulture), StringComparison.Ordinal)))
        {
            return false;
        }

        site.HomeRoute = route;
        await _siteService.UpdateSiteSettingsAsync(site);
        return true;
    }
}
