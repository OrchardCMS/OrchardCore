using System.IO.Hashing;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement;

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
    public static async Task PopulateMenuAsync(IShapeFactory shapeFactory, IShape parentShape, IShape menu, IEnumerable<MenuItem> menuItems, ViewContext viewContext)
    {
        await PopulateMenuLevelAsync(shapeFactory, parentShape, menu, menuItems, viewContext);
        ApplySelection(parentShape);
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

        MarkAsSelectedIfMatchesPath(menuItem, menuItemShape, viewContext);

        foreach (var className in menuItem.Classes)
        {
            menuItemShape.Classes.Add(className);
        }

        return menuItemShape;
    }

    private static void MarkAsSelectedIfMatchesPath(MenuItem menuItem, NavigationItemViewModel menuItemShape, ViewContext viewContext)
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

        var hrefSegmentCount = CountPathSegments(hrefPath);
        var requestSegmentCount = CountPathSegments(requestPath);
        var matchingSegmentCount = CountLeadingMatchingPathSegments(requestPath, hrefPath);

        // Match only when the menu href is a full leading prefix of the current request path.
        // This prevents broader sections (e.g. /Admin/*) from becoming selected on /Admin itself
        // when no direct menu item actually matches the current page.
        if (matchingSegmentCount > 0 && matchingSegmentCount == hrefSegmentCount && requestSegmentCount >= hrefSegmentCount)
        {
            // Score by matching leading segments so routes with more shared context
            // (like a specific content item id) outrank broader ancestors.
            menuItemShape.Score += matchingSegmentCount * 2;

            // Exact path match gets a small additional boost.
            if (hrefSegmentCount == requestSegmentCount)
            {
                menuItemShape.Score += 1;
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

    private static int CountPathSegments(string path)
    {
        return path.Split('/', StringSplitOptions.RemoveEmptyEntries).Length;
    }

    private static int CountLeadingMatchingPathSegments(string requestPath, string hrefPath)
    {
        var requestSegments = requestPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var hrefSegments = hrefPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        var max = Math.Min(requestSegments.Length, hrefSegments.Length);
        var matchCount = 0;

        for (var i = 0; i < max; i++)
        {
            if (!requestSegments[i].Equals(hrefSegments[i], StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            matchCount++;
        }

        return matchCount;
    }

    /// <summary>
    /// Ensures only one menuitem (and its ancestors) are marked as selected for the menu.
    /// </summary>
    /// <param name="parentShape">The menu shape.</param>
    private static void ApplySelection(IShape parentShape)
    {
        var selectedItem = GetHighestPrioritySelectedMenuItem(parentShape);

        if (selectedItem != null)
        {
            var ancestor = selectedItem.Parent;

            while (ancestor is NavigationItemViewModel ancestorItem)
            {
                ancestorItem.Selected = true;
                ancestor = ancestorItem.Parent;
            }
        }
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
    /// across restarts and load-balanced instances.
    /// </summary>
    private static string ComputeStableHash(string value)
        => XxHash32.HashToUInt32(MemoryMarshal.AsBytes(value.AsSpan())).ToString();

    private static int GetLevel(IShape shape)
        => shape is NavigationItemViewModel menuItemShape ? menuItemShape.Level : shape.GetProperty<int>(nameof(NavigationItemViewModel.Level));

    private static string GetHash(IShape shape)
        => shape is NavigationItemViewModel menuItemShape ? menuItemShape.Hash : shape.GetProperty<string>(nameof(NavigationItemViewModel.Hash));
}
