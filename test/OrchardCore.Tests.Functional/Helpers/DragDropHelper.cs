using Microsoft.Playwright;
using Xunit;

namespace OrchardCore.Tests.Functional.Helpers;

/// <summary>
/// Low-level mouse-simulated drag-and-drop primitive for the various
/// SortableJS-based UIs outside the Menu/Taxonomies hierarchy editor (see
/// SortableMenuHelper for that one specifically, which needs indent/outdent-
/// aware positioning) - Layers, AdminDashboard, AdminMenu's node tree, Flows
/// and Widgets all drive SortableJS the same basic way: mouse down on a
/// handle, move to a target, mouse up.
/// </summary>
public static class DragDropHelper
{
    public static async Task DragAsync(this IPage page, ILocator handle, ILocator target, int steps = 8)
    {
        var fromBox = await handle.BoundingBoxAsync();
        Assert.NotNull(fromBox);

        await page.Mouse.MoveAsync(fromBox.X + fromBox.Width / 2, fromBox.Y + fromBox.Height / 2);
        await page.Mouse.DownAsync();
        // A small initial move is needed to cross SortableJS's own drag-start
        // threshold before the real move toward the target, mirroring
        // SortableMenuHelper's approach for the same reason.
        await page.Mouse.MoveAsync(fromBox.X + fromBox.Width / 2, fromBox.Y + fromBox.Height / 2 + 5, new MouseMoveOptions { Steps = 3 });
        await page.WaitForTimeoutAsync(80);

        var targetBox = await target.BoundingBoxAsync();
        Assert.NotNull(targetBox);
        await page.Mouse.MoveAsync(targetBox.X + targetBox.Width / 2, targetBox.Y + targetBox.Height / 2, new MouseMoveOptions { Steps = steps });
        await page.WaitForTimeoutAsync(200);
        await page.Mouse.UpAsync();
        await page.WaitForTimeoutAsync(250);
    }
}
