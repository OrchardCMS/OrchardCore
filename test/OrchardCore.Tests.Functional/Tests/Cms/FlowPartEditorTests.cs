using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers the editors of the widgets added to a FlowPart via the AJAX "Add Widget" dropdown
// (src/OrchardCore.Modules/OrchardCore.Flows/Assets/ts/flows.edit.ts). Their scripts are loaded
// with the page (FlowPart.Edit.cshtml renders a mock editor of each widget type for that) and a
// module script is only ever evaluated once per page, so the editors inserted later must be
// picked up by the scripts themselves, e.g. through the shared observeAndInit() bloom helper.
public sealed class FlowPartEditorTests : CmsTestBase<BlogFixture>, IClassFixture<BlogFixture>
{
    public FlowPartEditorTests(BlogFixture fixture) : base(fixture) { }

    private static ILocator WidgetsOfType(IPage page, string contentType)
        => page.Locator($".widget-template-placeholder-flowpart .widget.widget-editor.card[data-content-type='{contentType}']");

    private static async Task AddWidgetAsync(IPage page, string contentType, int expectedCountAfter)
    {
        await page.Locator(".btn-widget-add-below .dropdown-toggle").ClickAsync();
        await page.Locator($".dropdown-item.add-widget[data-widget-type='{contentType}']").ClickAsync();
        await Assertions.Expect(page.Locator(".widget-template-placeholder-flowpart .widget-template")).ToHaveCountAsync(expectedCountAfter);
    }

    [Fact]
    public async Task AddWidget_ImageWidgets_MountMediaFields()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentTypes/Page/Create");
        await page.Locator(".widget-template-placeholder-flowpart").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });

        var consoleErrors = page.CollectConsoleErrors();

        // Two instances, so that the second one can only be mounted by the observer once the
        // media picker module has already handled the first one.
        await AddWidgetAsync(page, "Image", 1);
        await AddWidgetAsync(page, "Image", 2);

        var widgets = WidgetsOfType(page, "Image");
        await Assertions.Expect(widgets).ToHaveCountAsync(2);

        for (var i = 0; i < 2; i++)
        {
            var mediaField = widgets.Nth(i).Locator("[data-media-field-type]");
            await Assertions.Expect(mediaField).ToHaveAttributeAsync("data-media-field-mounted", "true");
            await Assertions.Expect(mediaField.Locator(".mf-toolbar")).ToBeVisibleAsync();
        }

        Assert.Empty(consoleErrors);

        await page.CloseAsync();
    }

    [Fact]
    public async Task AddWidget_ParagraphWidget_RegistersMediaPicker()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentTypes/Page/Create");
        await page.Locator(".widget-template-placeholder-flowpart").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });

        await AddWidgetAsync(page, "Paragraph", 1);

        // The media picker config element the HTML editors' media button relies on is only
        // rendered with the first editor of a request, i.e. with the widget fetched via AJAX.
        var config = WidgetsOfType(page, "Paragraph").Locator("[data-media-picker-config]");
        await Assertions.Expect(config).ToHaveCountAsync(1);

        await page.WaitForFunctionAsync("() => typeof document.querySelector('[data-media-picker-config]')?.__openMediaPicker === 'function'");

        await page.CloseAsync();
    }

    [Fact]
    public async Task AddWidget_BlockquoteWidget_RendersNoEmptyHint()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentTypes/Page/Create");
        await page.Locator(".widget-template-placeholder-flowpart").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });

        await AddWidgetAsync(page, "Blockquote", 1);

        var widget = WidgetsOfType(page, "Blockquote");
        await Assertions.Expect(widget.Locator("textarea")).ToBeVisibleAsync();

        // The Quote field has no hint configured.
        await Assertions.Expect(widget.Locator(".field-wrapper .hint")).ToHaveCountAsync(0);

        await page.CloseAsync();
    }
}
