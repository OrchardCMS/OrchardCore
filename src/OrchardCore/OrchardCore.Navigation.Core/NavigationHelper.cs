using System.Text.Json.Nodes;
using System.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Navigation;

public static class NavigationHelper
{
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
    public static async Task PopulateMenuAsync(dynamic shapeFactory, dynamic parentShape, dynamic menu, IEnumerable<MenuItem> menuItems, ViewContext viewContext)
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
    public static async Task PopulateMenuLevelAsync(dynamic shapeFactory, dynamic parentShape, dynamic menu, IEnumerable<MenuItem> menuItems, ViewContext viewContext)
    {
        foreach (var menuItem in menuItems)
        {
            dynamic menuItemShape = await BuildMenuItemShapeAsync(shapeFactory, parentShape, menu, menuItem, viewContext);

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
    private static async Task<dynamic> BuildMenuItemShapeAsync(dynamic shapeFactory, dynamic parentShape, dynamic menu, MenuItem menuItem, ViewContext viewContext)
    {
        var menuItemShape = (await shapeFactory.NavigationItem())
            .Text(menuItem.Text)
            .Href(menuItem.Href)
            .Target(menuItem.Target)
            .Url(menuItem.Url)
            .LinkToFirstChild(menuItem.LinkToFirstChild)
            .RouteValues(menuItem.RouteValues)
            .Item(menuItem)
            .Menu(menu)
            .Parent(parentShape)
            .Level(parentShape.Level == null ? 1 : (int)parentShape.Level + 1)
            .Priority(menuItem.Priority)
            .Local(menuItem.LocalNav)
            .Hash((parentShape.Hash + menuItem.Text.Value).GetHashCode().ToString())
            .Score(0);

        menuItemShape.Id = menuItem.Id;

        MarkAsSelectedIfMatchesQueryOrCookie(menuItem, menuItemShape, viewContext);

        foreach (var className in menuItem.Classes)
        {
            menuItemShape.Classes.Add(className);
        }

        return menuItemShape;
    }

    private static void MarkAsSelectedIfMatchesQueryOrCookie(MenuItem menuItem, dynamic menuItemShape, ViewContext viewContext)
    {
        if (string.IsNullOrEmpty(menuItem.Href) || menuItem.Href[0] != '/')
        {
            menuItemShape.Selected = menuItemShape.Score > 0;
            return;
        }

        // Strip query string from the menu item href to get a pure path for comparison.
        var hrefSpan = menuItem.Href.AsSpan();
        var queryIndex = hrefSpan.IndexOf('?');
        var hrefPath = (queryIndex >= 0 ? hrefSpan[..queryIndex] : hrefSpan).ToString();

        var requestPath = viewContext.HttpContext.Request.Path.Value ?? "/";
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

    /// <summary>
    /// Ensures only one menuitem (and its ancestors) are marked as selected for the menu.
    /// </summary>
    /// <param name="parentShape">The menu shape.</param>
    /// <param name="viewContext">The current <see cref="ViewContext"/>.</param>
    private static void ApplySelection(dynamic parentShape, ViewContext viewContext)
    {
        var selectedItem = GetHighestPrioritySelectedMenuItem(parentShape);

        // Persist the selected item hash inside the existing admin preferences cookie so
        // that direct URL navigations (not triggered by a nav click) also update the stored
        // selection, keeping the fallback in sync with URL-path-matched selections.
        if (selectedItem != null)
        {
            UpdateSelectedNavHashInPrefs(viewContext, selectedItem.Hash);

            while (selectedItem.Parent != null)
            {
                selectedItem = selectedItem.Parent;
                selectedItem.Selected = true;
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
        var key = $"{ShellScope.Context.Settings.Name}-adminPreferences";
        var raw = viewContext.HttpContext.Request.Cookies[key];

        if (string.IsNullOrEmpty(raw))
        {
            return null;
        }

        try
        {
            var json = Uri.UnescapeDataString(raw);
            var node = JsonNode.Parse(json);
            return node?["selectedNavHash"]?.GetValue<string>();
        }
        catch
        {
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

        viewContext.HttpContext.Response.Cookies.Append(key, Uri.EscapeDataString(prefs.ToJsonString()));
    }

    /// <summary>
    /// Traverses the menu and returns the selected item with the highest priority.
    /// </summary>
    /// <param name="parentShape">The menu shape.</param>
    /// <returns>The selected menu item shape.</returns>
    private static dynamic GetHighestPrioritySelectedMenuItem(dynamic parentShape)
    {
        dynamic result = null;

        var tempStack = new Stack<dynamic>(new dynamic[] { parentShape });

        while (tempStack.Count > 0)
        {
            // evaluate first
            dynamic item = tempStack.Pop();

            if (item.Selected == true)
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
            foreach (var i in item.Items)
            {
                tempStack.Push(i);
            }
        }

        return result;
    }
}
