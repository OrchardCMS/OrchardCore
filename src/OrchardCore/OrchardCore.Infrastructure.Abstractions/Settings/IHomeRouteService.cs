using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Settings;

/// <summary>
/// Updates the tenant home route and refreshes cached site settings when it changes.
/// </summary>
public interface IHomeRouteService
{
    /// <summary>
    /// Applies an update to a copy of the current route and saves it only when its values change.
    /// </summary>
    /// <param name="update">A trusted, in-process operation that modifies the route values.</param>
    /// <returns>Whether the route was changed and saved.</returns>
    Task<bool> UpdateAsync(Action<RouteValueDictionary> update);
}
