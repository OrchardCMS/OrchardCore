using System.Text.RegularExpressions;
using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers the FlowPart widget lifecycle actions that FlowsWidgetDragTests doesn't (that test is
// scoped to drag reordering only): adding a widget via the AJAX "Add Widget" dropdown, collapsing
// it, and deleting it via the confirm-dialog flow - all wired up in
// src/OrchardCore.Modules/OrchardCore.Flows/Assets/ts/flows.edit.ts's delegated document-level
// click listeners (".add-widget", ".widget-editor-btn-toggle", ".widget-delete") rather than
// FlowsWidgetDragTests' SortableJS drag path.
public sealed class FlowPartWidgetLifecycleTests : CmsTestBase<BlogFixture>, IClassFixture<BlogFixture>
{
    public FlowPartWidgetLifecycleTests(BlogFixture fixture) : base(fixture) { }

    [Fact]
    public async Task FlowPartWidget_AddCollapseAndDelete_UpdatesEditorState()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentTypes/Page/Create");
        await page.Locator("#TitlePart_Title").FillAsync("Flow Widget Lifecycle Test Page");

        // The placeholder starts empty (no widgets yet), which collapses its rendered height to
        // zero - wait for it to merely exist rather than for the default (visible) state, same as
        // FlowsWidgetDragTests.
        var placeholder = page.Locator(".widget-template-placeholder-flowpart");
        await placeholder.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });

        var addWidgetDropdown = page.Locator(".btn-widget-add-below .dropdown-toggle");
        var widgets = placeholder.Locator(".widget-template");

        // Add: the AJAX-fetched widget editor markup is inserted and the placeholder count grows.
        await addWidgetDropdown.ClickAsync();
        var widgetType = await page.Locator(".dropdown-item.add-widget").First.GetAttributeAsync("data-widget-type");
        await page.Locator(".dropdown-item.add-widget").First.ClickAsync();
        await Assertions.Expect(widgets).ToHaveCountAsync(1);

        var widget = widgets.First;
        var widgetEditor = widget.Locator(".widget.widget-editor.card");
        await Assertions.Expect(widgetEditor).ToHaveAttributeAsync("data-content-type", widgetType!);

        // Collapse: the toggle button flips the "collapsed" class on the widget's own .widget-editor
        // element (not the outer .widget-template wrapper drag reordering targets). Two buttons -
        // .widget-editor-btn-collapse and -expand - exist simultaneously (CSS shows only one
        // depending on state), so target them by their specific class rather than the shared
        // .widget-editor-btn-toggle one both carry.
        await Assertions.Expect(widgetEditor).Not.ToHaveClassAsync(new Regex(@"(^|\s)collapsed(\s|$)"));

        await widget.Locator(".widget-editor-btn-collapse").ClickAsync();
        await Assertions.Expect(widgetEditor).ToHaveClassAsync(new Regex(@"(^|\s)collapsed(\s|$)"));

        await widget.Locator(".widget-editor-btn-expand").ClickAsync();
        await Assertions.Expect(widgetEditor).Not.ToHaveClassAsync(new Regex(@"(^|\s)collapsed(\s|$)"));

        // Delete: goes through TheAdmin's confirmDialog() modal (see TheAdmin.ts) rather than
        // removing immediately - #modalOkButton is the dialog's confirm action.
        await widget.Locator(".widget-delete").ClickAsync();

        var confirmModal = page.Locator("#confirmRemoveModal");
        await Assertions.Expect(confirmModal).ToBeVisibleAsync();

        await page.Locator("#modalOkButton").ClickAsync();
        await Assertions.Expect(widgets).ToHaveCountAsync(0);

        await page.CloseAsync();
    }
}
