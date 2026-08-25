using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers OrchardCore.Seo's SeoMetaPart custom meta tags editor (seo-meta-part.ts, a Vue 3
// options-table-editor.ts consumer migrated in PR #19774), which is addable via the
// Flow/Bag "Add Widget" AJAX flow - the same double-instance-init bug class PR #19442
// fixed for other views, and the same options-table-editor.ts this session's Task 2 fixed
// a real vuedraggable.default regression in (see FormsFieldEditorTests.cs) - this test
// exercises a DIFFERENT consumer of that shared component to extend coverage.
public sealed class SeoMetaPartTests : CmsTestBase<SeoMetaPartTestsFixture>, IClassFixture<SeoMetaPartTestsFixture>
{
    public SeoMetaPartTests(SeoMetaPartTestsFixture fixture) : base(fixture) { }

    private static ILocator WidgetOfType(IPage page, string contentType)
        => page.Locator($".widget.widget-editor.card[data-content-type='{contentType}']");

    [Fact]
    public async Task AjaxInjectedSeoMetaPart_CustomMetaTag_PersistsAcrossReload()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentTypes/SeoMetaPartTestPage/Create");
        await page.Locator("#TitlePart_Title").FillAsync("Seo Meta Part AJAX Test");

        var placeholder = page.Locator(".widget-template-placeholder-flowpart");
        await placeholder.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });
        await page.Locator(".btn-widget-add-below .dropdown-toggle").ClickAsync();
        await page.Locator("a.dropdown-item.add-widget[data-widget-type='SeoMetaPartTestWidget']").ClickAsync();

        var widget = WidgetOfType(page, "SeoMetaPartTestWidget");
        await Assertions.Expect(widget).ToHaveCountAsync(1);

        // Wait for the Vue app to actually mount in the AJAX-injected instance (the
        // options table itself only exists once Vue.createApp(...).mount() has run).
        var optionsTable = widget.Locator(".options-table");
        await Assertions.Expect(optionsTable).ToHaveCountAsync(1);
        await page.WaitForTimeoutAsync(300); // Widget-add card-collapse/insert transition.

        // SeoMetaPart's editor renders on its own "SEO" tab within the widget card
        // (separate from the default "Content" tab) - switch to it before interacting.
        await widget.GetByRole(AriaRole.Tab, new LocatorGetByRoleOptions { Name = "SEO" }).ClickAsync();

        var addRowLink = widget.Locator(".options-table-editor-mount a.btn-light");
        var rows = optionsTable.Locator("tbody tr");
        await Assertions.Expect(rows).ToHaveCountAsync(0);

        await addRowLink.ClickAsync();
        await Assertions.Expect(rows).ToHaveCountAsync(1);

        // Columns are content, name, property, httpEquiv, charset (in that order) - fill
        // in the first two (content, name) to prove real per-column data binding, not
        // just a row appearing.
        var textInputs = rows.Nth(0).Locator("input[type='text']");
        await textInputs.Nth(0).FillAsync("test-meta-content");
        await textInputs.Nth(1).FillAsync("test-meta-name");

        await page.Locator(".btn.publish").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentItems/SeoMetaPartTestPage");
        await page.Locator("li.list-group-item").Filter(new LocatorFilterOptions { HasText = "Seo Meta Part AJAX Test" })
            .Locator("a.edit").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var reloadedWidget = WidgetOfType(page, "SeoMetaPartTestWidget");
        await Assertions.Expect(reloadedWidget).ToHaveCountAsync(1);
        await reloadedWidget.GetByRole(AriaRole.Tab, new LocatorGetByRoleOptions { Name = "SEO" }).ClickAsync();

        var reloadedRow = reloadedWidget.Locator(".options-table tbody tr").First;
        var reloadedInputs = reloadedRow.Locator("input[type='text']");
        await Assertions.Expect(reloadedInputs.Nth(0)).ToHaveValueAsync("test-meta-content");
        await Assertions.Expect(reloadedInputs.Nth(1)).ToHaveValueAsync("test-meta-name");

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }
}
