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
    // once it has actually been reached, re-reading the target's CURRENT
    // position on every retry (see WaitForSortableOrderAsync's comment for why
    // a fixed coordinate goes stale) rather than nudging around a fixed point.
    public static async Task DragMenuItemJustAfterAsync(this IPage page, string itemText, string targetText)
    {
        var fromBox = await MenuItem(page, itemText).Locator(".menu-item-title").BoundingBoxAsync();
        Assert.NotNull(fromBox);

        await page.Mouse.MoveAsync(fromBox.X + fromBox.Width / 2, fromBox.Y + fromBox.Height / 2);
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync(fromBox.X + fromBox.Width / 2, fromBox.Y - 10, new MouseMoveOptions { Steps = 5 });
        await page.WaitForTimeoutAsync(150);

        await page.WaitForSortableOrderAsync(itemText, targetText, expectAfter: true);

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

        await page.WaitForSortableOrderAsync(itemText, targetText, expectAfter: false);

        await page.Mouse.UpAsync();
        await page.WaitForTimeoutAsync(250);
    }

    // Polls the live #menu li.menu-item order (SortableJS reorders the real DOM
    // mid-drag under forceFallback) until `itemText`'s element is immediately
    // next to `targetText`'s on the requested side, moving the pointer toward
    // the target's CURRENT bounding box on every retry rather than a single
    // coordinate captured once before the loop. A fixed coordinate goes stale
    // the moment the first swap happens: reordering physically moves every row
    // below the swap point (e.g. dragging an item to just-after a target
    // shifts that target's own row up to take the dragged item's old slot),
    // so a pointer sitting at the target's pre-swap position is no longer
    // anywhere near its current one and can never trigger a further swap from
    // there - confirmed live: this was consistently landing one position off
    // in every failure (e.g. 'Earth -> Exploration -> Space' instead of
    // 'Earth -> Space -> Exploration'), not intermittently, once the
    // animation-timing noise that had been masking it was removed (see
    // OrchardTestFixture's Sortable.create animation: 0 patch).
    private static async Task WaitForSortableOrderAsync(this IPage page, string itemText, string targetText, bool expectAfter)
    {
        // 50 attempts x up to ~300ms/attempt = up to ~15s of active polling,
        // generous relative to the ~30s+ margins other tests in this same CI
        // environment have needed under genuine runner contention (see
        // ShortcodeModalTests/PredefinedListEditorTests history).
        const int maxAttempts = 50;

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

            // Re-read the target's CURRENT position every time, not once before the
            // loop - see this method's own comment for why a stale one stops working
            // after the very first swap. Pulling the pointer well clear of the
            // target's far edge and back to its near edge is guaranteed to cross
            // SortableJS's swap threshold from the correct direction regardless of
            // how small its swap zone is.
            var targetBox = await MenuItem(page, targetText).BoundingBoxAsync();

            if (targetBox is null)
            {
                // The target row can briefly not match any element while SortableJS's
                // fallback clone is mid-transition; skip this attempt rather than fail.
                await page.WaitForTimeoutAsync(100);
                continue;
            }

            var x = targetBox.X + targetBox.Width / 2;
            var nearY = expectAfter ? targetBox.Y + targetBox.Height - 3 : targetBox.Y + 3;
            var awayY = expectAfter ? targetBox.Y + targetBox.Height + 40 : targetBox.Y - 40;

            await page.Mouse.MoveAsync(x, awayY, new MouseMoveOptions { Steps = 3 });
            await page.WaitForTimeoutAsync(100);
            await page.Mouse.MoveAsync(x, nearY, new MouseMoveOptions { Steps = 3 });
            await page.WaitForTimeoutAsync(150);
        }

        Assert.Fail(
            $"SortableJS never reordered '{itemText}' to be immediately {(expectAfter ? "after" : "before")} '{targetText}' " +
            $"after {maxAttempts} polling attempts. Current order: " +
            await page.EvaluateAsync<string>("() => Array.from(document.querySelectorAll('#menu li.menu-item')).map(el => el.textContent.trim().split('\\n')[0].trim()).join(' -> ')"));
    }

    public static Task<string> GetMenuItemDepthAsync(this IPage page, string itemText)
        => MenuItem(page, itemText).GetAttributeAsync("data-depth");
}
