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
    //
    // SortableJS runs with forceFallback: true, which means it reorders the
    // REAL DOM live, in response to dragover events, WHILE the mouse button is
    // still down - the final order is decided before mouseup, not by mouseup
    // itself. That makes the actual insertion point directly observable
    // mid-drag, so instead of trusting a fixed settle delay before releasing
    // (which under CI-runner CPU contention was observed to release too early,
    // one build up: an item dragged to just after its target deterministically
    // landed just before it instead - a wrong final result, not a slow-to-
    // arrive correct one, so no fixed timeout size can be relied on to avoid
    // it), poll the live DOM for the expected order and only release the mouse
    // once it has actually been reached, nudging the pointer slightly on each
    // retry to keep feeding SortableJS fresh dragover events.
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
        await page.Mouse.MoveAsync(targetBox.X + targetBox.Width / 2, targetBox.Y + targetBox.Height / 2, new MouseMoveOptions { Steps = 10 });
        await page.WaitForTimeoutAsync(150);

        var dropX = targetBox.X + targetBox.Width / 2;
        var dropY = targetBox.Y + targetBox.Height - 3;
        await page.Mouse.MoveAsync(dropX, dropY, new MouseMoveOptions { Steps = 8 });
        await page.WaitForItemImmediatelyAfterAsync(itemText, targetText, dropX, dropY);

        await page.Mouse.UpAsync();
        await page.WaitForTimeoutAsync(250);
    }

    // Drags the item to just above `targetText` - the mirror image of
    // DragMenuItemJustAfterAsync, needed when the item being moved sits below
    // its target (e.g. bringing the last item above the first): nudging the
    // initial pick-up downward first, rather than upward, keeps the pointer
    // safely inside the list's bounds when dragging an item that has nothing
    // above it yet to move into.
    //
    // Same poll-before-release approach as DragMenuItemJustAfterAsync, for the
    // same reason (see its comment).
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
        await page.Mouse.MoveAsync(targetBox.X + targetBox.Width / 2, targetBox.Y + targetBox.Height / 2, new MouseMoveOptions { Steps = 10 });
        await page.WaitForTimeoutAsync(150);

        var dropX = targetBox.X + targetBox.Width / 2;
        var dropY = targetBox.Y + 3;
        await page.Mouse.MoveAsync(dropX, dropY, new MouseMoveOptions { Steps = 8 });
        await page.WaitForItemImmediatelyBeforeAsync(itemText, targetText, dropX, dropY);

        await page.Mouse.UpAsync();
        await page.WaitForTimeoutAsync(250);
    }

    // Polls the live #menu li.menu-item order (SortableJS reorders the real
    // DOM mid-drag under forceFallback) until `itemText`'s element is
    // immediately followed by `targetText`'s, nudging the pointer by a
    // sub-pixel amount on each retry to keep feeding SortableJS fresh
    // dragover events - a stationary pointer stops producing them entirely,
    // so simply waiting longer without nudging would never make progress.
    private static Task WaitForItemImmediatelyAfterAsync(this IPage page, string itemText, string targetText, float x, float y)
        => page.WaitForSortableOrderAsync(itemText, targetText, expectAfter: true, x, y);

    private static Task WaitForItemImmediatelyBeforeAsync(this IPage page, string itemText, string targetText, float x, float y)
        => page.WaitForSortableOrderAsync(itemText, targetText, expectAfter: false, x, y);

    private static async Task WaitForSortableOrderAsync(this IPage page, string itemText, string targetText, bool expectAfter, float x, float y)
    {
        // 150 attempts x 100ms = up to 15s of polling - generous relative to the
        // ~30s+ delays observed elsewhere in this same CI environment under
        // genuine contention (see ShortcodeModalTests/PredefinedListEditorTests
        // history), since each failed attempt here also re-feeds a dragover
        // event rather than just idling.
        const int maxAttempts = 150;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var inOrder = await page.EvaluateAsync<bool>(
                """
                ([itemText, targetText, expectAfter]) => {
                    const items = Array.from(document.querySelectorAll('#menu li.menu-item'));
                    const itemIndex = items.findIndex((el) => el.textContent.includes(itemText));
                    const targetIndex = items.findIndex((el) => el.textContent.includes(targetText));

                    if (itemIndex === -1 || targetIndex === -1) {
                        return false;
                    }

                    return expectAfter ? itemIndex === targetIndex + 1 : itemIndex === targetIndex - 1;
                }
                """,
                new object[] { itemText, targetText, expectAfter });

            if (inOrder)
            {
                return;
            }

            // Nudge by a small amount - big enough to be a distinct pointer position
            // (so it can't be coalesced away as a no-op) but well within the target
            // item's own row so it can never drift the drop point onto a neighbour,
            // alternating direction so it can't drift the drop point away from where
            // we actually want it.
            var nudge = attempt % 2 == 0 ? 3 : -3;
            await page.Mouse.MoveAsync(x, y + nudge);
            await page.WaitForTimeoutAsync(100);
        }

        Assert.Fail(
            $"SortableJS never reordered '{itemText}' to be immediately {(expectAfter ? "after" : "before")} '{targetText}' " +
            $"after {maxAttempts} polling attempts.");
    }

    public static Task<string> GetMenuItemDepthAsync(this IPage page, string itemText)
        => MenuItem(page, itemText).GetAttributeAsync("data-depth");
}
