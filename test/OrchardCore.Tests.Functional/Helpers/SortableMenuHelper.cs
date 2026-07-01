using Microsoft.Playwright;
using Xunit;

namespace OrchardCore.Tests.Functional.Helpers;

/// <summary>
/// Drives the SortableJS-based nested drag-and-drop hierarchy editor shared by
/// OrchardCore.Menu and OrchardCore.Taxonomies (@orchardcore/bloom's
/// sortable-menu.ts): "#menu" &gt; "li.menu-item[data-depth]" items, dragged via
/// their ".menu-item-title" handle.
/// </summary>
public static class SortableMenuHelper
{
    private static ILocator MenuItem(IPage page, string itemText)
        => page.Locator("#menu li.menu-item").Filter(new LocatorFilterOptions { HasText = itemText }).First;

    // Drags the item purely sideways from its own position, by deltaX pixels -
    // enough (see INDENT_THRESHOLD in sortable-menu.ts) to request an indent
    // (positive) or outdent (negative), without repositioning vertically first.
    public static async Task DragMenuItemSidewaysAsync(this IPage page, string itemText, float deltaX)
    {
        var box = await MenuItem(page, itemText).Locator(".menu-item-title").BoundingBoxAsync();
        Assert.NotNull(box);

        await page.Mouse.MoveAsync(box.X + box.Width / 2, box.Y + box.Height / 2);
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync(box.X + box.Width / 2 + Math.Sign(deltaX) * 10, box.Y + box.Height / 2, new MouseMoveOptions { Steps = 3 });
        await page.WaitForTimeoutAsync(80);
        await page.Mouse.MoveAsync(box.X + box.Width / 2 + deltaX, box.Y + box.Height / 2, new MouseMoveOptions { Steps = 10 });
        await page.WaitForTimeoutAsync(150);
        await page.Mouse.UpAsync();
        await page.WaitForTimeoutAsync(250);
    }

    // Drags the item to just below `targetText`, repositioning it vertically
    // without an explicit sideways nudge - exercising sortable-menu.ts's "clamp
    // the item's original depth to whatever's still valid at its new position"
    // behavior, rather than an explicit indent/outdent gesture.
    public static async Task DragMenuItemJustAfterAsync(this IPage page, string itemText, string targetText)
    {
        var fromBox = await MenuItem(page, itemText).Locator(".menu-item-title").BoundingBoxAsync();
        Assert.NotNull(fromBox);

        await page.Mouse.MoveAsync(fromBox.X + fromBox.Width / 2, fromBox.Y + fromBox.Height / 2);
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync(fromBox.X + fromBox.Width / 2, fromBox.Y - 10, new MouseMoveOptions { Steps = 5 });
        await page.WaitForTimeoutAsync(150);

        var targetBox = await MenuItem(page, targetText).BoundingBoxAsync();
        Assert.NotNull(targetBox);
        await page.Mouse.MoveAsync(targetBox.X + targetBox.Width / 2, targetBox.Y + targetBox.Height - 3, new MouseMoveOptions { Steps = 8 });
        await page.WaitForTimeoutAsync(200);
        await page.Mouse.UpAsync();
        await page.WaitForTimeoutAsync(250);
    }

    // Drags the item to just above `targetText` - the mirror image of
    // DragMenuItemJustAfterAsync, needed when the item being moved sits below
    // its target (e.g. bringing the last item above the first): nudging the
    // initial pick-up downward first, rather than upward, keeps the pointer
    // safely inside the list's bounds when dragging an item that has nothing
    // above it yet to move into.
    public static async Task DragMenuItemJustBeforeAsync(this IPage page, string itemText, string targetText)
    {
        var fromBox = await MenuItem(page, itemText).Locator(".menu-item-title").BoundingBoxAsync();
        Assert.NotNull(fromBox);

        await page.Mouse.MoveAsync(fromBox.X + fromBox.Width / 2, fromBox.Y + fromBox.Height / 2);
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync(fromBox.X + fromBox.Width / 2, fromBox.Y + 10, new MouseMoveOptions { Steps = 5 });
        await page.WaitForTimeoutAsync(150);

        var targetBox = await MenuItem(page, targetText).BoundingBoxAsync();
        Assert.NotNull(targetBox);
        await page.Mouse.MoveAsync(targetBox.X + targetBox.Width / 2, targetBox.Y + 3, new MouseMoveOptions { Steps = 8 });
        await page.WaitForTimeoutAsync(200);
        await page.Mouse.UpAsync();
        await page.WaitForTimeoutAsync(250);
    }

    public static Task<string> GetMenuItemDepthAsync(this IPage page, string itemText)
        => MenuItem(page, itemText).GetAttributeAsync("data-depth");
}
