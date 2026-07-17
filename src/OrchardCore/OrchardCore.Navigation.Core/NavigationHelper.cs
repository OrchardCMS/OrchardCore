using System.IO.Hashing;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Navigation;

public static class NavigationHelper
{
    // Selection ranks, highest wins. A menu item linking directly to the requested page is the
    // most specific match there is and must win over the path a page declares as its owner, which
    // in turn must beat any prefix match, whose rank is the segment count of the matched href.
    // The gaps between tiers exceed any realistic path depth, so exact always beats declared,
    // which always beats prefix.
    private const int ExactRequestMatchScore = 200;
    private const int DeclaredMatchScore = 100;

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

        MarkAsSelected(menuItem, menuItemShape, viewContext);

        foreach (var className in menuItem.Classes)
        {
            menuItemShape.Classes.Add(className);
        }

        return menuItemShape;
    }

    /// <summary>
    /// Ranks a menu item against the current request, deterministically and without any stored
    /// state. From strongest to weakest: a menu item linking to the requested page itself, the
    /// path the page declares as its owner (see
    /// <see cref="NavigationHttpContextExtensions.SetNavigationSelectionPath"/>), and finally a
    /// URL prefix match. Disambiguating between items that resolve to the same rank (e.g. two
    /// links with the same href) is a per-tab concern handled client-side by the admin theme.
    /// </summary>
    private static void MarkAsSelected(MenuItem menuItem, NavigationItemViewModel menuItemShape, ViewContext viewContext)
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
        // page of its content type). It only decides pages that no menu item links to directly,
        // so it never overrides a menu item pointing at the requested page itself.
        var declaredPath = viewContext.HttpContext.GetNavigationSelectionPath();
        var selectionPath = declaredPath is null
            ? requestPath
            : RemovePathBase(declaredPath, viewContext.HttpContext.Request.PathBase);

        var segmentCount = hrefPath.Split('/', StringSplitOptions.RemoveEmptyEntries).Length;

        if (requestPath.Equals(hrefPath, StringComparison.OrdinalIgnoreCase))
        {
            // The menu item links to the requested page itself, e.g. an admin menu link pointing
            // at a specific content item. Nothing identifies the page more precisely than that.
            menuItemShape.Score = ExactRequestMatchScore + segmentCount;
        }
        else if (selectionPath.Equals(hrefPath, StringComparison.OrdinalIgnoreCase))
        {
            // The page declared this menu item as the one owning it.
            menuItemShape.Score = DeclaredMatchScore + segmentCount;
        }
        else if (segmentCount > 0 && selectionPath.StartsWith(hrefPath.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase))
        {
            // Prefix match (e.g. "/Admin/ContentTypes" matches "/Admin/ContentTypes/Edit/Blog").
            // Deeper prefix = higher score, ensuring the most specific ancestor wins.
            menuItemShape.Score = segmentCount;
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
    /// Traverses the menu and returns the single selected item, resolving ties by rank
    /// (<see cref="NavigationItemViewModel.Score"/>), then by <see cref="MenuItem.Priority"/>,
    /// then by menu order (the first matching item wins). Items that lose the tie are unselected.
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
                // Higher rank wins; on equal rank, higher priority wins. Equal rank and priority
                // keeps the item found first in menu order (children are pushed in reverse below).
                else if (item.Score > result.Score || (item.Score == result.Score && item.Priority > result.Priority))
                {
                    result.Selected = false;
                    result = item;
                }
                else
                {
                    item.Selected = false;
                }
            }

            // Push children in reverse so the stack pops them in menu order, making the first
            // item in menu order win an otherwise-equal tie.
            for (var i = shape.Items.Count - 1; i >= 0; i--)
            {
                if (shape.Items[i] is IShape child)
                {
                    tempStack.Push(child);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Computes a deterministic hash identifying a menu item across requests. Unlike
    /// <see cref="string.GetHashCode()"/>, which is randomized per process, the result is stable
    /// across restarts and load-balanced instances. It is rendered as the <c>data-admin-hash</c>
    /// attribute so the admin theme can recognise the clicked item on the next page and break
    /// ties between items that resolve to the same rank.
    /// </summary>
    private static string ComputeStableHash(string value)
        => XxHash32.HashToUInt32(MemoryMarshal.AsBytes(value.AsSpan())).ToString();

    private static int GetLevel(IShape shape)
        => shape is NavigationItemViewModel menuItemShape ? menuItemShape.Level : shape.GetProperty<int>(nameof(NavigationItemViewModel.Level));

    private static string GetHash(IShape shape)
        => shape is NavigationItemViewModel menuItemShape ? menuItemShape.Hash : shape.GetProperty<string>(nameof(NavigationItemViewModel.Hash));
}
