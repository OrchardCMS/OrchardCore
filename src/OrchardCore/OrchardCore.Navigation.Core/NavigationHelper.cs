using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Navigation;

public static class NavigationHelper
{
    private const string SelectedNavHashCacheKey = "OrchardCore.Navigation.SelectedNavHash";
    private static readonly object SelectedNavHashMissing = new();

    public static bool UseLegacyFormat()
    {
        return AppContext.TryGetSwitch(NavigationConstants.LegacyAdminMenuNavigationSwitchKey, out var enable) && enable;
    }

    /// <summary>
    /// Populates the menu shapes.
    /// </summary>
    /// <param name="shapeFactory">The shape factory.</param>
    /// <param name="parentShape">The menu parent shape.</param>
    /// <param name="menu">The menu shape.</param>
    /// <param name="menuItems">The current level to populate.</param>
    /// <param name="viewContext">The current <see cref="ViewContext"/>.</param>
    public static async Task PopulateMenuAsync(IShapeFactory shapeFactory, IShape parentShape, IShape menu, IEnumerable<MenuItem> menuItems, ViewContext viewContext)
    {
        await PopulateMenuLevelAsync(shapeFactory, parentShape, menu, menuItems, viewContext);
        ApplySelection(parentShape, viewContext);
    }

    /// <summary>
    /// Populates the menu shapes for the level recursively.
    /// </summary>
    /// <param name="shapeFactory">The shape factory.</param>
    /// <param name="parentShape">The menu parent shape.</param>
    /// <param name="menu">The menu shape.</param>
    /// <param name="menuItems">The current level to populate.</param>
    /// <param name="viewContext">The current <see cref="ViewContext"/>.</param>
    public static async Task PopulateMenuLevelAsync(IShapeFactory shapeFactory, IShape parentShape, IShape menu, IEnumerable<MenuItem> menuItems, ViewContext viewContext)
    {
        foreach (var menuItem in menuItems)
        {
            var menuItemShape = await BuildMenuItemShapeAsync(shapeFactory, parentShape, menu, menuItem, viewContext);

            if (menuItem.Items != null && menuItem.Items.Count > 0)
            {
                await PopulateMenuLevelAsync(shapeFactory, menuItemShape, menu, menuItem.Items, viewContext);
            }

            await parentShape.AddAsync(menuItemShape, menuItem.Position);
        }
    }

    /// <summary>
    /// Builds a menu item shape.
    /// </summary>
    /// <param name="shapeFactory">The shape factory.</param>
    /// <param name="parentShape">The parent shape.</param>
    /// <param name="menu">The menu shape.</param>
    /// <param name="menuItem">The menu item to build the shape for.</param>
    /// <param name="viewContext">The current <see cref="ViewContext"/>.</param>
    /// <returns>The menu item shape.</returns>
    private static async Task<NavigationItemViewModel> BuildMenuItemShapeAsync(IShapeFactory shapeFactory, IShape parentShape, IShape menu, MenuItem menuItem, ViewContext viewContext)
    {
        var menuItemShape = (NavigationItemViewModel)await shapeFactory.CreateAsync<NavigationItemViewModel, (MenuItem, IShape, IShape)>("NavigationItem", static (shape, state) =>
        {
            var (menuItem, parentShape, menu) = state;
            shape.Text = menuItem.Text;
            shape.Href = menuItem.Href;
            shape.Target = menuItem.Target;
            shape.Url = menuItem.Url;
            shape.LinkToFirstChild = menuItem.LinkToFirstChild;
            shape.RouteValues = menuItem.RouteValues;
            shape.Item = menuItem;
            shape.Menu = menu;
            shape.Parent = parentShape;
            shape.Level = GetLevel(parentShape) + 1;   
            shape.Priority = menuItem.Priority;
            shape.Local = menuItem.LocalNav;
            shape.Hash = (GetHash(parentShape) + menuItem.Text.Value).GetHashCode().ToString();
            shape.Score = 0;
        }, (menuItem, parentShape, menu));

        menuItemShape.Id = menuItem.Id;

        MarkAsSelectedIfMatchesPathOrPreferences(menuItem, menuItemShape, viewContext);

        foreach (var className in menuItem.Classes)
        {
            menuItemShape.Classes.Add(className);
        }

        return menuItemShape;
    }

    private static void MarkAsSelectedIfMatchesPathOrPreferences(MenuItem menuItem, NavigationItemViewModel menuItemShape, ViewContext viewContext)
    {
        if (string.IsNullOrEmpty(menuItem.Href) || menuItem.Href[0] != '/')
        {
            menuItemShape.Selected = menuItemShape.Score > 0;
            return;
        }

        // Strip query string from the menu item href to get a pure path for comparison.
        var hrefSpan = menuItem.Href.AsSpan();
        var queryIndex = hrefSpan.IndexOf('?');
        var hrefPath = RemovePathBase(
            (queryIndex >= 0 ? hrefSpan[..queryIndex] : hrefSpan).ToString(),
            viewContext.HttpContext.Request.PathBase);

        var requestPath = RemovePathBase(
            viewContext.HttpContext.Request.Path.Value ?? "/",
            viewContext.HttpContext.Request.PathBase);
        var segmentCount = hrefPath.Split('/', StringSplitOptions.RemoveEmptyEntries).Length;

        if (requestPath.Equals(hrefPath, StringComparison.OrdinalIgnoreCase))
        {
            // Exact URL match — score by path depth so deeper (more specific) links beat
            // shallower ones that share the same prefix.
            menuItemShape.Score += segmentCount + 2;
        }
        else if (segmentCount > 0 && requestPath.StartsWith(hrefPath.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase))
        {
            // Prefix match (e.g. "/Admin/ContentTypes" matches "/Admin/ContentTypes/Edit/Blog").
            // Deeper prefix = higher score, ensuring the most specific ancestor wins.
            menuItemShape.Score += segmentCount;
        }
        else if (IsAncestorPath(requestPath, hrefPath))
        {
            // Ancestor paths (e.g. "/Admin" while evaluating "/Admin/Features") are not
            // within the menu item branch and should not reuse a stale clicked hash.
        }
        else
        { 
            // No URL match — fall back to the selectedNavHash stored in the admin preferences
            // cookie, which JS writes proactively when the user clicks a nav link.
            var selectedNavHash = GetSelectedNavHashFromPrefs(viewContext);

            if (selectedNavHash == menuItemShape.Hash)
            {
                menuItemShape.Score++;
            }
        }

        menuItemShape.Selected = menuItemShape.Score > 0;
    }

    private static string RemovePathBase(string path, PathString pathBase)
    {
        if (!pathBase.HasValue || pathBase == PathString.Empty)
        {
            return path;
        }

        var pathBaseValue = pathBase.Value.TrimEnd('/');

        if (path.Equals(pathBaseValue, StringComparison.OrdinalIgnoreCase))
        {
            return "/";
        }

        if (path.StartsWith(pathBaseValue + "/", StringComparison.OrdinalIgnoreCase))
        {
            return path[pathBaseValue.Length..];
        }

        return path;
    }

    private static bool IsAncestorPath(string requestPath, string hrefPath)
    {
        var normalizedRequestPath = requestPath.AsSpan().TrimEnd('/');
        var normalizedHrefPath = hrefPath.AsSpan().TrimEnd('/');

        if (normalizedRequestPath.IsEmpty)
        {
            normalizedRequestPath = "/";
        }

        if (normalizedHrefPath.IsEmpty || normalizedRequestPath.Length >= normalizedHrefPath.Length)
        {
            return false;
        }

        return normalizedHrefPath[normalizedRequestPath.Length] == '/'
            && normalizedHrefPath.StartsWith(normalizedRequestPath, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Ensures only one menuitem (and its ancestors) are marked as selected for the menu.
    /// </summary>
    /// <param name="parentShape">The menu shape.</param>
    /// <param name="viewContext">The current <see cref="ViewContext"/>.</param>
    private static void ApplySelection(IShape parentShape, ViewContext viewContext)
    {
        var selectedItem = GetHighestPrioritySelectedMenuItem(parentShape);

        // Persist the selected item hash inside the existing admin preferences cookie so
        // that direct URL navigations (not triggered by a nav click) also update the stored
        // selection, keeping the fallback in sync with URL-path-matched selections.
        if (selectedItem != null)
        {
            UpdateSelectedNavHashInPrefs(viewContext, selectedItem.Hash);

            var ancestor = selectedItem.Parent;

            while (ancestor is NavigationItemViewModel ancestorItem)
            {
                ancestorItem.Selected = true;
                ancestor = ancestorItem.Parent;
            }
        }
    }

    /// <summary>
    /// Reads the <c>selectedNavHash</c> field from the admin preferences cookie.
    /// The cookie is written by JS (js-cookie) using <c>encodeURIComponent</c>, so the
    /// raw value must be URL-decoded before JSON parsing.
    /// </summary>
    private static string GetSelectedNavHashFromPrefs(ViewContext viewContext)
    {
        if (viewContext.HttpContext.Items.TryGetValue(SelectedNavHashCacheKey, out var cached))
        {
            return ReferenceEquals(cached, SelectedNavHashMissing) ? null : cached as string;
        }

        var key = $"{ShellScope.Context.Settings.Name}-adminPreferences";
        var raw = viewContext.HttpContext.Request.Cookies[key];

        if (string.IsNullOrEmpty(raw))
        {
            viewContext.HttpContext.Items[SelectedNavHashCacheKey] = SelectedNavHashMissing;
            return null;
        }

        try
        {
            var json = Uri.UnescapeDataString(raw);
            var node = JsonNode.Parse(json);
            var selectedNavHash = node?["selectedNavHash"]?.GetValue<string>();
            viewContext.HttpContext.Items[SelectedNavHashCacheKey] = selectedNavHash ?? SelectedNavHashMissing;
            return selectedNavHash;
        }
        catch
        {
            viewContext.HttpContext.Items[SelectedNavHashCacheKey] = SelectedNavHashMissing;
            return null;
        }
    }

    /// <summary>
    /// Merges the <c>selectedNavHash</c> field into the existing admin preferences cookie,
    /// preserving all other fields (e.g. <c>leftSidebarCompact</c>).
    /// The value is URL-encoded to match the encoding js-cookie uses when reading.
    /// </summary>
    private static void UpdateSelectedNavHashInPrefs(ViewContext viewContext, string hash)
    {
        var key = $"{ShellScope.Context.Settings.Name}-adminPreferences";
        var raw = viewContext.HttpContext.Request.Cookies[key];

        JsonObject prefs;

        try
        {
            var json = string.IsNullOrEmpty(raw) ? "{}" : Uri.UnescapeDataString(raw);
            prefs = JsonNode.Parse(json)?.AsObject() ?? new JsonObject();
        }
        catch
        {
            prefs = new JsonObject();
        }

        prefs["selectedNavHash"] = hash;

        var options = new CookieOptions
        {
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(360),
            MaxAge = TimeSpan.FromDays(360),
        };

        viewContext.HttpContext.Response.Cookies.Append(key, Uri.EscapeDataString(prefs.ToJsonString()), options);
        viewContext.HttpContext.Items[SelectedNavHashCacheKey] = hash ?? SelectedNavHashMissing;
    }

    /// <summary>
    /// Traverses the menu and returns the selected item with the highest priority.
    /// </summary>
    /// <param name="parentShape">The menu shape.</param>
    /// <returns>The selected menu item shape.</returns>
    private static NavigationItemViewModel GetHighestPrioritySelectedMenuItem(IShape parentShape)
    {
        NavigationItemViewModel result = null;

        var tempStack = new Stack<IShape>([parentShape]);

        while (tempStack.Count > 0)
        {
            // evaluate first
            var shape = tempStack.Pop();

            if (shape is NavigationItemViewModel item && item.Selected)
            {
                if (result == null) // found the first one
                {
                    result = item;
                }
                else // found more selected: tie break required.
                {
                    if (item.Score > result.Score)
                    {
                        result.Selected = false;
                        result = item;
                    }
                    else if (item.Priority > result.Priority)
                    {
                        result.Selected = false;
                        result = item;
                    }
                    else
                    {
                        item.Selected = false;
                    }
                }
            }

            // add children to the stack to be evaluated too
            foreach (var child in shape.Items.OfType<IShape>())
            {
                tempStack.Push(child);
            }
        }

        return result;
    }

    private static int GetLevel(IShape shape)
        => shape is NavigationItemViewModel menuItemShape ? menuItemShape.Level : shape.GetProperty<int>(nameof(NavigationItemViewModel.Level));

    private static string GetHash(IShape shape)
        => shape is NavigationItemViewModel menuItemShape ? menuItemShape.Hash : shape.GetProperty<string>(nameof(NavigationItemViewModel.Hash));
}
