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

        // SortableJS ignores drag-over events while one of its reorder animations is still running, so the
        // last move of a multi-step glide is easily swallowed and the drop then settles wherever the step
        // before it happened to land. Which step that is comes down to the exact pixel offsets involved, so
        // an unrelated layout change anywhere above the list silently flips the outcome. Nudge the cursor
        // once the animations have settled so the final position is processed. The nudge stays on the
        // coordinates the glide ended on rather than following the target element, which the reorder may
        // well have moved out from under the cursor - chasing it would just drag the item back again.
        var dropX = targetBox.X + targetBox.Width / 2;
        var dropY = targetBox.Y + targetBox.Height / 2;

        for (var i = 0; i < 3; i++)
        {
            await page.WaitForTimeoutAsync(200);
            await page.Mouse.MoveAsync(dropX, dropY - 1);
            await page.Mouse.MoveAsync(dropX, dropY);
        }

        await page.WaitForTimeoutAsync(200);
        await page.Mouse.UpAsync();
        await page.WaitForTimeoutAsync(250);
    }
}
