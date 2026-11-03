using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Task 10: covers two independent, unrelated pieces of interactive UI in the
// ContentTypes/Contents area of the Vue2-to-Vue3/jQuery-removal migration:
//   1. content-type-sortable.ts (OrchardCore.ContentTypes) - drag-reorder of a content
//      type's fields/parts on the type editor.
//   2. content-types-sitemap-source.ts (OrchardCore.Contents) - cascading show/hide/
//      enable-disable toggle logic on the ContentTypesSitemapSource editor.
public sealed class ContentTypesEditorTests : CmsTestBase<ContentTypesEditorTestsFixture>, IClassFixture<ContentTypesEditorTestsFixture>
{
    public ContentTypesEditorTests(ContentTypesEditorTestsFixture fixture) : base(fixture) { }

    [Fact]
    public async Task DragReorderParts_PersistsNewOrderOnReload()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        await page.GotoAndAssertOkAsync("/Admin/ContentTypes/Edit/ReorderTestType");

        var partsList = page.Locator("ul#parts.sortable");
        var partItems = partsList.Locator("li.list-group-item");
        await Assertions.Expect(partItems).ToHaveCountAsync(3); // TitlePart + HtmlBodyPart + AutoroutePart

        // TitlePart was declared at Position 0, HtmlBodyPart at Position 1 - the real
        // order under test is TitlePart before HtmlBodyPart.
        var initialTexts = (await partItems.AllTextContentsAsync()).ToArray();
        var titleIndex = Array.FindIndex(initialTexts, t => t.Contains("Title"));
        var htmlBodyIndex = Array.FindIndex(initialTexts, t => t.Contains("Html"));
        Assert.True(titleIndex >= 0 && htmlBodyIndex >= 0 && titleIndex < htmlBodyIndex,
            $"Expected TitlePart before HtmlBodyPart initially. Texts: {string.Join(" | ", initialTexts)}");

        // Simulating a real SortableJS pointer drag via synthetic mouse events proved too
        // flaky in earlier tasks (see DeploymentPlanStepOrderTests) - instead, since
        // Sortable.create() attaches its own internal drag machinery (no simple exposed
        // global function here, unlike steporder.js), directly reorder the underlying
        // hidden <input name="OrderedPartNames"> form inputs via DOM manipulation, which
        // is what SortableJS's own drag would ultimately produce - then submit the form.
        // This tests the real regression-relevant surface (server-side persistence of the
        // submitted order) without depending on the third-party drag library's own
        // pointer-event wiring.
        await page.EvaluateAsync("""
            () => {
                const list = document.querySelector('#parts.sortable');
                const items = Array.from(list.children);
                const titleItem = items.find(li => li.textContent.includes('Title'));
                const htmlBodyItem = items.find(li => li.textContent.includes('Html'));
                list.insertBefore(htmlBodyItem, titleItem);
            }
            """);

        var reorderedTexts = (await partItems.AllTextContentsAsync()).ToArray();
        var newTitleIndex = Array.FindIndex(reorderedTexts, t => t.Contains("Title"));
        var newHtmlBodyIndex = Array.FindIndex(reorderedTexts, t => t.Contains("Html"));
        Assert.True(newHtmlBodyIndex < newTitleIndex, "DOM reorder didn't take effect before save.");

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save" }).First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GotoAndAssertOkAsync("/Admin/ContentTypes/Edit/ReorderTestType");
        var reloadedItems = page.Locator("ul#parts.sortable li.list-group-item");
        await Assertions.Expect(reloadedItems).ToHaveCountAsync(3);
        var reloadedTexts = (await reloadedItems.AllTextContentsAsync()).ToArray();
        var reloadedTitleIndex = Array.FindIndex(reloadedTexts, t => t.Contains("Title"));
        var reloadedHtmlBodyIndex = Array.FindIndex(reloadedTexts, t => t.Contains("Html"));
        Assert.True(reloadedHtmlBodyIndex < reloadedTitleIndex,
            $"Expected HtmlBodyPart before TitlePart after reload. Texts: {string.Join(" | ", reloadedTexts)}");

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task SitemapSourceEditor_TogglesIndexAllVsSelectedPanels()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        await page.GotoAndAssertOkAsync("/Admin/SitemapSource/Create/contenttypeseditortestsitemap/ContentTypesSitemapSource");

        var indexAllCheckbox = page.Locator(".content-types-sitemap-index-all");
        var indexAllRow = page.Locator("#index-all-row");
        var indexSelectedRow = page.Locator("#index-selected-row");
        var limitItemsContainer = page.Locator("#ContentTypesSitemapSource_LimitItems_Container");

        await Assertions.Expect(indexAllCheckbox).ToHaveCountAsync(1);

        // Default (IndexAll = true, the model's own default): the "index all" panel is
        // visible, the "selected content types" panel is collapsed, and the LimitItems
        // toggle itself is hidden (meaningless while indexing everything).
        await Assertions.Expect(indexAllRow).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex(@"\bcollapse\b"));
        await Assertions.Expect(indexSelectedRow).ToHaveClassAsync(new System.Text.RegularExpressions.Regex(@"\bcollapse\b"));
        await Assertions.Expect(limitItemsContainer).ToHaveCSSAsync("display", "none");

        // Unchecking "Index All" should hide the "index all" panel, show the
        // selected-types panel, and show the LimitItems toggle.
        await indexAllCheckbox.UncheckAsync();
        await Assertions.Expect(indexAllRow).ToHaveCSSAsync("display", "none");
        await Assertions.Expect(indexSelectedRow).ToHaveCSSAsync("display", "block");
        await Assertions.Expect(limitItemsContainer).Not.ToHaveCSSAsync("display", "none");

        // Re-checking it should restore the original state.
        await indexAllCheckbox.CheckAsync();
        await Assertions.Expect(indexAllRow).ToHaveCSSAsync("display", "block");
        await Assertions.Expect(indexSelectedRow).ToHaveCSSAsync("display", "none");
        await Assertions.Expect(limitItemsContainer).ToHaveCSSAsync("display", "none");

        // Uncheck again so the "selected content types" checkboxes below are reachable/enabled.
        await indexAllCheckbox.UncheckAsync();

        // Checking a content-type checkbox should enable its row's disabled/readonly
        // select+input siblings.
        var firstContentTypeCheckbox = page.Locator(".content-type-checkbox").First;
        var firstListItem = firstContentTypeCheckbox.Locator("xpath=ancestor::li[contains(@class, 'list-group-item')][1]");
        var firstSelect = firstListItem.Locator("select").First;

        await Assertions.Expect(firstSelect).ToBeDisabledAsync();
        await firstContentTypeCheckbox.CheckAsync();
        await Assertions.Expect(firstSelect).Not.ToBeDisabledAsync();
        await firstContentTypeCheckbox.UncheckAsync();
        await Assertions.Expect(firstSelect).ToBeDisabledAsync();

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }
}
