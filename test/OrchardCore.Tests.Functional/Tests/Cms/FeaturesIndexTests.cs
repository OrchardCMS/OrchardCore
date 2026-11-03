using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers OrchardCore.Features' features-index.ts, a fully rewritten interactive admin
// script (search/filter, per-group select-all, badge show-more, keyboard Escape
// handling) converted from onDocumentReady/jQuery in PR #19522. This exact class of bug
// already shipped once and was only caught manually (commit f5533d29bd on that PR): three
// independent attribute/value mismatches between features-index.ts and Features.cshtml
// (data-search-text vs. the real data-filter-value, wrong status-filter option values, a
// visibility-filter checkbox .value collision) together made the page render as
// permanently empty regardless of filter state.
//
// A second, separate instance of this same bug class was caught and fixed while writing
// this test (see issue #19772): features-index.ts looked up "#features-summary",
// ".feature-group-select-all", ".feature-group-select-all-label",
// ".feature-group-select-all-text", and ".feature-group-toggle-container", none of which
// exist in Features.cshtml (the real markup uses "#list-summary" and the
// ".list-group-select-all*" class family, shared with the generic list-management.js
// helper - see src/docs/reference/modules/Resources/README.md's "List management"
// section). Because updateFeatureGroupSelectAllState() returns early when either
// querySelector call comes back null, this silently disabled the per-group select-all
// checkbox (never checked/unchecked/indeterminate), the group-hidden-when-empty toggle,
// and the "Showing X of Y features" summary text - while leaving the search/filter
// d-none toggling itself working, since that part doesn't depend on the broken
// selectors. Fixed by renaming features-index.ts's selectors to match the real markup.
public sealed class FeaturesIndexTests : CmsTestBase<BlogFixture>, IClassFixture<BlogFixture>
{
    public FeaturesIndexTests(BlogFixture fixture) : base(fixture) { }

    private static ILocator FeatureGroups(IPage page)
        => page.Locator(".feature-group");

    [Fact]
    public async Task Search_PartialModuleName_FiltersToMatchingFeaturesOnly()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        await page.GotoAndAssertOkAsync("/Admin/Features");

        var allFeatureItems = page.Locator(".list-group-item[data-filter-value]");
        var visibleItems = allFeatureItems.Locator("visible=true");

        // The visibility filter defaults to hiding on-demand and always-enabled features,
        // so "visible by default" is a strict subset of "all items" - capture the actual
        // default-visible count rather than assuming every item starts visible.
        var visibleCountBeforeFilter = await visibleItems.CountAsync();
        Assert.True(visibleCountBeforeFilter > 1, "Expected more than one visible feature item on a stock CMS setup.");

        var searchBox = page.Locator("#search-box");
        await searchBox.FillAsync("Markdown");

        // The regression this guards against made EVERY item disappear regardless of
        // filter text, so asserting "some but not all" items remain visible is the
        // meaningful check, not just ">= 1".
        await Assertions.Expect(visibleItems).Not.ToHaveCountAsync(0);
        await Assertions.Expect(visibleItems).Not.ToHaveCountAsync(visibleCountBeforeFilter);

        // The Markdown feature/module must be among the still-visible items.
        await Assertions.Expect(visibleItems.Filter(new LocatorFilterOptions { HasText = "Markdown" }))
            .Not.ToHaveCountAsync(0);

        await searchBox.FillAsync(string.Empty);
        await Assertions.Expect(visibleItems).ToHaveCountAsync(visibleCountBeforeFilter);

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task SearchBox_EscapeKey_ClearsFilterAndRestoresAllFeatures()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        await page.GotoAndAssertOkAsync("/Admin/Features");

        var allFeatureItems = page.Locator(".list-group-item[data-filter-value]");
        var visibleItems = allFeatureItems.Locator("visible=true");
        var visibleCountBeforeFilter = await visibleItems.CountAsync();

        var searchBox = page.Locator("#search-box");
        await searchBox.FillAsync("Markdown");
        await Assertions.Expect(visibleItems).Not.ToHaveCountAsync(visibleCountBeforeFilter);

        await searchBox.PressAsync("Escape");

        await Assertions.Expect(searchBox).ToHaveValueAsync(string.Empty);
        await Assertions.Expect(visibleItems).ToHaveCountAsync(visibleCountBeforeFilter);

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task SelectAllWithinGroup_ChecksOnlyThatGroupsFeatures_AndUpdatesSummaryText()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        await page.GotoAndAssertOkAsync("/Admin/Features");

        // Find a group whose select-all checkbox is actually enabled (not disabled, which
        // happens when the group has zero selectable - i.e. non-always-enabled,
        // non-on-demand - features) and has at least 2 selectable feature checkboxes, so
        // the "other groups unaffected" assertion below is meaningful.
        var groups = FeatureGroups(page);
        var groupCount = await groups.CountAsync();
        ILocator targetGroup = null;
        var targetGroupCheckboxes = default(ILocator);

        for (var i = 0; i < groupCount; i++)
        {
            var candidate = groups.Nth(i);
            var candidateSelectAll = candidate.Locator(".list-group-select-all");
            if (await candidateSelectAll.CountAsync() == 0 || await candidateSelectAll.IsDisabledAsync())
            {
                continue;
            }

            var candidateCheckboxes = candidate.Locator("input[name='featureIds']");
            if (await candidateCheckboxes.CountAsync() >= 2)
            {
                targetGroup = candidate;
                targetGroupCheckboxes = candidateCheckboxes;
                break;
            }
        }

        Assert.NotNull(targetGroup);

        var selectAllCheckbox = targetGroup.Locator(".list-group-select-all");

        // The bug this test guards against made this querySelector-dependent state
        // machine a permanent no-op - assert the checkbox is not already checked/
        // indeterminate before interacting, so a regression back to "does nothing"
        // can't slip through as a false pass.
        await Assertions.Expect(selectAllCheckbox).Not.ToBeCheckedAsync();

        await selectAllCheckbox.CheckAsync();

        // Checking the group-level checkbox is itself a native DOM operation that
        // succeeds regardless of whether the JS listener wiring it up to the individual
        // feature checkboxes is broken - so the real assertion is that the CASCADE
        // actually happened (every feature checkbox in the group became checked), not
        // just that the group checkbox itself is checked.
        var checkboxCount = await targetGroupCheckboxes.CountAsync();
        for (var i = 0; i < checkboxCount; i++)
        {
            await Assertions.Expect(targetGroupCheckboxes.Nth(i)).ToBeCheckedAsync();
        }

        // Checkboxes OUTSIDE this group must remain unchecked - this is the exact "value
        // collision"/cross-group-leak class of bug this whole feature guards against.
        var allCheckboxesCount = await page.Locator("input[name='featureIds']").CountAsync();
        var checkedCount = await page.Locator("input[name='featureIds']:checked").CountAsync();
        Assert.Equal(checkboxCount, checkedCount);
        Assert.True(allCheckboxesCount >= checkboxCount);

        // The per-group summary text (".list-group-select-all-text") must reflect the new
        // checked count - this is the exact text-update logic the selector mismatch
        // silently broke.
        var summaryText = await targetGroup.Locator(".list-group-select-all-text").TextContentAsync();
        Assert.Contains(checkboxCount.ToString(), summaryText);

        await selectAllCheckbox.UncheckAsync();
        for (var i = 0; i < checkboxCount; i++)
        {
            await Assertions.Expect(targetGroupCheckboxes.Nth(i)).Not.ToBeCheckedAsync();
        }

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }
}
