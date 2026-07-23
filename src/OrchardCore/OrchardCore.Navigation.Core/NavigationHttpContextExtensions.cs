using Microsoft.AspNetCore.Http;

namespace OrchardCore.Navigation;

public static class NavigationHttpContextExtensions
{
    private const string NavigationSelectionPathKey = "OrchardCore.Navigation.SelectionPath";

    /// <summary>
    /// Declares the path menu selection should be computed against for the current request,
    /// instead of the request path. Use this on pages whose URL does not correspond to a menu
    /// item but that logically belong to one, e.g. a content item edit page declaring the list
    /// page of its content type. The path should be generated through routing (e.g.
    /// <c>Url.Action</c>) so it matches the menu item href exactly; any query string is ignored.
    /// </summary>
    /// <param name="httpContext">The current <see cref="HttpContext"/>.</param>
    /// <param name="path">The path of the menu item owning the current page.</param>
    public static void SetNavigationSelectionPath(this HttpContext httpContext, string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            httpContext.Items.Remove(NavigationSelectionPathKey);
            return;
        }

        // Menu selection only compares paths, so a query string would prevent any match.
        var queryIndex = path.IndexOf('?');

        httpContext.Items[NavigationSelectionPathKey] = queryIndex >= 0 ? path[..queryIndex] : path;
    }

    /// <summary>
    /// Gets the path declared via <see cref="SetNavigationSelectionPath"/> for the current
    /// request, or <c>null</c> when none was declared.
    /// </summary>
    /// <param name="httpContext">The current <see cref="HttpContext"/>.</param>
    public static string GetNavigationSelectionPath(this HttpContext httpContext)
        => httpContext.Items.TryGetValue(NavigationSelectionPathKey, out var path) ? path as string : null;
}
