using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers the SortableJS-based widget drag-and-drop on FlowPart editors
// (src/OrchardCore.Modules/OrchardCore.Flows/Views/FlowPart.Edit.cshtml),
// persisted as part of the normal content-item form save (unlike Layers/
// AdminMenu, there's no separate AJAX call per move). No shipped recipe
// ships a Page with more than one FlowPart widget already placed, so this
// test creates one and adds two (differently typed, so they're
// distinguishable afterward) widgets via the admin UI itself before
// testing the drag.
public sealed class FlowsWidgetDragTests : CmsTestBase<BlogFixture>, IClassFixture<BlogFixture>
{
    public FlowsWidgetDragTests(BlogFixture fixture) : base(fixture) { }

    private static ILocator ContentTypeOf(ILocator widget)
        => widget.Locator(".widget.widget-editor.card[data-content-type]");

    [Fact]
    public async Task FlowsWidgetDrag_ReorderTwoWidgets_PersistsOnSave()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentTypes/Page/Create");
        await page.Locator("#TitlePart_Title").FillAsync("Flows Drag Test Page");

        // The placeholder starts empty (no widgets yet), which collapses its
        // rendered height to zero - wait for it to merely exist rather than
        // for the default (visible) state.
        var placeholder = page.Locator(".widget-template-placeholder-flowpart");
        await placeholder.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });

        // The button only has an icon (no visible text - its "Add Widget" label
        // is just a localized title/tooltip attribute), so it's targeted
        // structurally instead.
        var addWidgetDropdown = page.Locator(".btn-widget-add-below .dropdown-toggle");
        var widgets = placeholder.Locator(".widget-template");

        // Add the first two distinct widget types offered, so the two
        // resulting widgets can be told apart by content type afterward.
        await addWidgetDropdown.ClickAsync();
        var firstOptionType = await page.Locator(".dropdown-item.add-widget").Nth(0).GetAttributeAsync("data-widget-type");
        await page.Locator(".dropdown-item.add-widget").Nth(0).ClickAsync();
        await Assertions.Expect(widgets).ToHaveCountAsync(1);

        await addWidgetDropdown.ClickAsync();
        var secondOptionType = await page.Locator(".dropdown-item.add-widget").Nth(1).GetAttributeAsync("data-widget-type");
        await page.Locator(".dropdown-item.add-widget").Nth(1).ClickAsync();
        await Assertions.Expect(widgets).ToHaveCountAsync(2);

        await Assertions.Expect(ContentTypeOf(widgets.Nth(0))).ToHaveAttributeAsync("data-content-type", firstOptionType);
        await Assertions.Expect(ContentTypeOf(widgets.Nth(1))).ToHaveAttributeAsync("data-content-type", secondOptionType);

        await page.DragAsync(widgets.Nth(1).Locator(".widget-editor-handle"), widgets.Nth(0));

        // The drag itself should have already swapped their DOM order.
        await Assertions.Expect(ContentTypeOf(widgets.Nth(0))).ToHaveAttributeAsync("data-content-type", secondOptionType);
        await Assertions.Expect(ContentTypeOf(widgets.Nth(1))).ToHaveAttributeAsync("data-content-type", firstOptionType);

        await page.Locator(".btn.draft").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Creating (unlike editing) a content item redirects straight to its
        // own new Edit page rather than a returnUrl-based list - reload it
        // explicitly to verify the order was actually persisted, rather than
        // just reflecting the just-submitted in-memory DOM state.
        await page.GotoAndAssertOkAsync(page.Url);

        var reloadedWidgets = page.Locator(".widget-template-placeholder-flowpart .widget-template");
        await Assertions.Expect(ContentTypeOf(reloadedWidgets.Nth(0))).ToHaveAttributeAsync("data-content-type", secondOptionType);
        await Assertions.Expect(ContentTypeOf(reloadedWidgets.Nth(1))).ToHaveAttributeAsync("data-content-type", firstOptionType);

        await page.CloseAsync();
    }
}
