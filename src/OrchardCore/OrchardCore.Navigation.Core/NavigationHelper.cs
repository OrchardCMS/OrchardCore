using System.IO.Hashing;
using System.Runtime.InteropServices;
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

    // Selection scoring tiers. A menu item linking directly to the requested page is the most
    // specific match there is and must win over the path a page declares as its owner, which in
    // turn must beat the clicked-item fallback, which must beat any prefix match, whose score is
    // the segment count of the matched href.
    private const int ExactRequestMatchScore = 200;
    private const int DeclaredMatchScore = 100;
    private const int SelectedNavHashScore = 50;

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
            shape.Hash = ComputeStableHash(GetHash(parentShape) + menuItem.Text.Value);
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

    /// <summary>
    /// Scores a menu item against the current request. Selection is resolved from the strongest
    /// to the weakest signal: a menu item linking to the requested page itself, the path the page
    /// declares as its owner (see
    /// <see cref="NavigationHttpContextExtensions.SetNavigationSelectionPath"/>), the last clicked
    /// menu item, and finally a URL prefix match.
    /// </summary>
    private static void MarkAsSelectedIfMatchesPathOrPreferences(MenuItem menuItem, NavigationItemViewModel menuItemShape, ViewContext viewContext)
    {
        if (string.IsNullOrEmpty(menuItem.Href) || menuItem.Href[0] != '/')
        {
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

        // A page can declare the menu path it belongs to (e.g. an edit page declaring the list
        // page of its content type). It only applies to pages that no menu item links to
        // directly, so it never overrides a menu item pointing at the requested page itself.
        var declaredPath = viewContext.HttpContext.GetNavigationSelectionPath();
        var selectionPath = declaredPath is null
            ? requestPath
            : RemovePathBase(declaredPath, viewContext.HttpContext.Request.PathBase);

        var segmentCount = hrefPath.Split('/', StringSplitOptions.RemoveEmptyEntries).Length;

        // The clicked-item fallback, restored from the admin preferences cookie written by the
        // admin theme. It only decides the selection for pages that neither declare a selection
        // path nor exactly match a menu item href. Ancestor paths (e.g. "/Admin" while evaluating
        // "/Admin/Features") are not within the menu item branch and must not reuse a stale hash.
        if (!IsAncestorPath(selectionPath, hrefPath) && GetSelectedNavHashFromPrefs(viewContext) == menuItemShape.Hash)
        {
            menuItemShape.Score += SelectedNavHashScore;
        }

        if (requestPath.Equals(hrefPath, StringComparison.OrdinalIgnoreCase))
        {
            // The menu item links to the requested page itself, e.g. an admin menu link pointing
            // at a specific content item. Nothing identifies the page more precisely than that.
            menuItemShape.Score += ExactRequestMatchScore + segmentCount;
        }
        else if (selectionPath.Equals(hrefPath, StringComparison.OrdinalIgnoreCase))
        {
            // The page declared this menu item as the one owning it.
            menuItemShape.Score += DeclaredMatchScore + segmentCount;
        }
        else if (segmentCount > 0 && selectionPath.StartsWith(hrefPath.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase))
        {
            // Prefix match (e.g. "/Admin/ContentTypes" matches "/Admin/ContentTypes/Edit/Blog").
            // Deeper prefix = higher score, ensuring the most specific ancestor wins.
            menuItemShape.Score += segmentCount;
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
    /// raw value must be URL-decoded before JSON parsing. This is a fallback for pages that
    /// do not declare a selection path; prefer
    /// <see cref="NavigationHttpContextExtensions.SetNavigationSelectionPath"/>.
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

    /// <summary>
    /// Computes a deterministic hash identifying a menu item across requests. Unlike
    /// <see cref="string.GetHashCode()"/>, which is randomized per process, the result is stable
    /// across restarts and load-balanced instances, so the hash persisted in the admin
    /// preferences cookie keeps matching the rendered menu.
    /// </summary>
    private static string ComputeStableHash(string value)
        => XxHash32.HashToUInt32(MemoryMarshal.AsBytes(value.AsSpan())).ToString();

    private static int GetLevel(IShape shape)
        => shape is NavigationItemViewModel menuItemShape ? menuItemShape.Level : shape.GetProperty<int>(nameof(NavigationItemViewModel.Level));

    private static string GetHash(IShape shape)
        => shape is NavigationItemViewModel menuItemShape ? menuItemShape.Hash : shape.GetProperty<string>(nameof(NavigationItemViewModel.Hash));
}
