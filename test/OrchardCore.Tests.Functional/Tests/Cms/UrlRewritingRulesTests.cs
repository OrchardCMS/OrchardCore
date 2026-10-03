using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers OrchardCore.UrlRewriting's rule list (/Admin/UrlRewriting/Index): the SortableJS
// drag-reorder wired up by sortable-rules.js (.ui-sortable-handle drag handle, POSTing to
// SortRulesEndpoint's /url-rewriting/resort via url-rewriting-admin-index.ts's
// sortingListManager.create call). BulkSelectListTests.cs already covers this page's
// select-all/bulk-action checkbox behavior (shared bulk-select-list.ts component) using the
// same 2 seeded rules; this test is the reorder-specific coverage the plan called for.
//
// NOTE ON SCOPE: the original plan text for this task also mentioned an enable/disable
// "toggle" - RewriteRule (OrchardCore.UrlRewriting.Abstractions/Models/RewriteRule.cs) has
// no Enabled/IsEnabled field at all, so that half of the task doesn't exist to test. Seeding
// uses the built-in "Redirect" IUrlRewriteRuleSource (UrlRedirectRuleSource, registered in
// OrchardCore.UrlRewriting's Startup.cs) - the same source bulk-select-list-tests.recipe.json
// already uses for its own 2 UrlRewriting rows.
public sealed class UrlRewritingRulesTests : CmsTestBase<UrlRewritingRulesTestsFixture>, IClassFixture<UrlRewritingRulesTestsFixture>
{
    public UrlRewritingRulesTests(UrlRewritingRulesTestsFixture fixture) : base(fixture) { }

    [Fact]
    public async Task UrlRewritingRules_DragReorder_PersistsNewOrder()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        await page.GotoAndAssertOkAsync("/Admin/UrlRewriting/Index");

        var rows = page.Locator("#rewrite-rules-sortable-list li.item");
        await Assertions.Expect(rows).ToHaveCountAsync(2);

        // Confirm the seeded, pre-reorder order: Rule One first, Rule Two second.
        await Assertions.Expect(rows.Nth(0)).ToContainTextAsync("Url Rewriting Rule One");
        await Assertions.Expect(rows.Nth(1)).ToContainTextAsync("Url Rewriting Rule Two");

        var firstHandle = rows.Nth(0).Locator(".ui-sortable-handle");
        var secondRow = rows.Nth(1);

        var firstBox = await firstHandle.BoundingBoxAsync();
        var secondBox = await secondRow.BoundingBoxAsync();
        Assert.NotNull(firstBox);
        Assert.NotNull(secondBox);

        // Drag the first row's handle past the second row's vertical midpoint so
        // SortableJS's onUpdate fires with oldIndex=0/newIndex=1, then POSTs the new
        // order to SortRulesEndpoint (url-rewriting/resort).
        await page.Mouse.MoveAsync(firstBox!.X + (firstBox.Width / 2), firstBox.Y + (firstBox.Height / 2));
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync(secondBox!.X + (secondBox.Width / 2), secondBox.Y + (secondBox.Height / 2) + 5, new MouseMoveOptions { Steps = 10 });
        await page.Mouse.UpAsync();

        await Assertions.Expect(rows.Nth(0)).ToContainTextAsync("Url Rewriting Rule Two");
        await Assertions.Expect(rows.Nth(1)).ToContainTextAsync("Url Rewriting Rule One");

        // Reload to confirm the reorder was actually persisted server-side (via the
        // resort endpoint), not just a client-side DOM move that reverts on refresh.
        await page.GotoAndAssertOkAsync("/Admin/UrlRewriting/Index");

        var reloadedRows = page.Locator("#rewrite-rules-sortable-list li.item");
        await Assertions.Expect(reloadedRows).ToHaveCountAsync(2);
        await Assertions.Expect(reloadedRows.Nth(0)).ToContainTextAsync("Url Rewriting Rule Two");
        await Assertions.Expect(reloadedRows.Nth(1)).ToContainTextAsync("Url Rewriting Rule One");

        await page.CloseAsync();
    }
}
