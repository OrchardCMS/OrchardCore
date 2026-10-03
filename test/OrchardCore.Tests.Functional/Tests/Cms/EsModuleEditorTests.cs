using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers the field/part editors converted from the onDocumentReady pattern to real ES
// modules (TextField-IconPicker.Edit.cshtml, HtmlField.Edit.cshtml (default CodeMirror)/
// -Wysiwyg/-Trumbowyg.Edit.cshtml, HtmlBodyPart-Wysiwyg/Trumbowyg.Edit.cshtml,
// LiquidPart.Edit.cshtml). Module scripts never listen for DOMContentLoaded, so unlike
// the onDocumentReady wrapper they need no verification on that front - what actually
// needs checking is (a) they still run at all on a normal full page load, and (b) they
// still run when injected after the fact via the FlowPart "Add Widget" AJAX flow
// (src/OrchardCore.Modules/OrchardCore.Flows/Assets/ts/flows.edit.ts), which is the
// scenario PR #19442 had to fix for the previous onDocumentReady-based views. Uses the
// EsModuleTests recipe (test/OrchardCore.Tests.Functional/Fixtures/
// es-module-tests.recipe.json), which no shipped recipe covers (it needs an explicit
// Trumbowyg HtmlField editor and an IconPicker field on a Widget-stereotype type,
// combined with a Trumbowyg and a Wysiwyg HtmlBodyPart on two more Widget types, plus a
// LiquidPart-attached Widget type).
//
// Also covers TextField-Color.Edit.cshtml, the first view extracted from an inline
// `type="module"` script into a real compiled .ts file (text-field-color.ts) that
// self-registers via the shared observeAndInit() bloom helper instead of a per-instance
// residual inline call. The AJAX test below adds two instances of the same widget type,
// which is exactly the scenario observeAndInit exists for: the module's own top-level
// `observeAndInit(".color_wrapper", initColorField)` call only ever runs once (module
// bodies are evaluated once per page), so the *second* instance is only initialized if
// the shared MutationObserver correctly picks up its later AJAX insertion.
public sealed class EsModuleEditorTests : CmsTestBase<EsModuleTestsFixture>, IClassFixture<EsModuleTestsFixture>
{
    public EsModuleEditorTests(EsModuleTestsFixture fixture) : base(fixture) { }

    private static ILocator WidgetOfType(IPage page, string contentType)
        => page.Locator($".widget.widget-editor.card[data-content-type='{contentType}']");

    // Both the Wysiwyg and Trumbowyg HtmlField/HtmlBodyPart editors end up calling
    // jQuery's .trumbowyg() plugin (just with different button sets), which wraps the
    // source <textarea> in a "trumbowyg-box" the moment it initializes successfully -
    // a reliable signal the module actually ran, without depending on toolbar button
    // markup that could vary by editor/options.
    private static ILocator TrumbowygBoxesOf(ILocator widget)
        => widget.Locator(".trumbowyg-box");

    // The icon picker plugin builds its "iconpicker-item" buttons into the field's
    // (initially empty) sibling .dropdown-menu synchronously during initialization
    // (fontawesome-iconpicker's _createPopover/_createIconpicker) - so their mere
    // presence proves the module ran, with no need to actually open the dropdown.
    private static ILocator IconPickerItemsOf(ILocator widget)
        => widget.Locator(".dropdown-menu .iconpicker-item");

    // CodeMirror.fromTextArea() wraps the source <textarea> in a ".CodeMirror" element the
    // moment it initializes successfully - covers HtmlField.Edit.cshtml's default editor
    // and LiquidPart.Edit.cshtml, both plain CodeMirror.fromTextArea() calls with no
    // wrapping settings object.
    private static ILocator CodeMirrorEditorsOf(ILocator widget)
        => widget.Locator(".CodeMirror");

    // The color field's wrapper div always exists statically in the markup, so its mere
    // presence proves nothing - text-field-color.ts's initColorField() sets its inline
    // background-color from the color/opacity inputs' values, so only a correctly-computed
    // background-color proves the module actually ran for *this* wrapper instance.
    private static ILocator ColorWrapperOf(ILocator widget)
        => widget.Locator(".color_wrapper");

    private static async Task AddWidgetAsync(IPage page, ILocator addWidgetDropdown, ILocator widgets, string contentType, int expectedCountAfter)
    {
        await addWidgetDropdown.ClickAsync();
        await page.Locator($".dropdown-item.add-widget[data-widget-type='{contentType}']").ClickAsync();
        await Assertions.Expect(widgets).ToHaveCountAsync(expectedCountAfter);
    }

    [Fact]
    public async Task FullPageLoad_SeededWidgetFieldsInitialize_NoConsoleErrors()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        // Collected only from here on - the login flow's own post-login redirect is
        // unrelated to what this test verifies and shouldn't be attributed to it.
        var consoleErrors = page.CollectConsoleErrors();

        // The seeded page already has the fields widget placed in its FlowPart, so
        // opening it for edit is a genuine full page load for that widget's editors -
        // no AJAX injection involved, unlike the second test below.
        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentItems/EsModuleTestPage");
        await page.Locator("li.list-group-item").Filter(new LocatorFilterOptions { HasText = "ES Module Test Page" })
            .Locator("a.edit").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var widget = WidgetOfType(page, "EsModuleTestFieldsWidget");
        await Assertions.Expect(widget).ToHaveCountAsync(1);

        // Three HtmlField instances (Wysiwyg + Trumbowyg + default CodeMirror editors) on
        // this one widget.
        await Assertions.Expect(TrumbowygBoxesOf(widget)).ToHaveCountAsync(2);
        await Assertions.Expect(CodeMirrorEditorsOf(widget)).ToHaveCountAsync(1);
        await Assertions.Expect(IconPickerItemsOf(widget)).Not.ToHaveCountAsync(0);

        // Seeded with "#336699" (rgb(51, 102, 153)).
        await Assertions.Expect(ColorWrapperOf(widget)).ToHaveCSSAsync("background-color", "rgb(51, 102, 153)");

        // The seeded Liquid widget is a separate widget in the same Flow, also present
        // from this same full page load.
        var liquidWidget = WidgetOfType(page, "EsModuleTestLiquidWidget");
        await Assertions.Expect(liquidWidget).ToHaveCountAsync(1);
        await Assertions.Expect(CodeMirrorEditorsOf(liquidWidget)).ToHaveCountAsync(1);

        Assert.Empty(consoleErrors);

        await page.CloseAsync();
    }

    [Fact]
    public async Task AjaxAddedWidgets_FieldsInitialize_NoConsoleErrors()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        // Collected only from here on - see the comment in the test above.
        var consoleErrors = page.CollectConsoleErrors();

        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentTypes/EsModuleTestPage/Create");
        await page.Locator("#TitlePart_Title").FillAsync("ES Module AJAX Test Page");

        var placeholder = page.Locator(".widget-template-placeholder-flowpart");
        await placeholder.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });

        var addWidgetDropdown = page.Locator(".btn-widget-add-below .dropdown-toggle");
        var widgets = placeholder.Locator(".widget-template");

        // Two instances of the same fields widget, added independently via AJAX - the
        // actual point of the ES module conversion is that each instance's module-scoped
        // "settings"/"defaultButtons"/etc. locals no longer collide with the other's.
        await AddWidgetAsync(page, addWidgetDropdown, widgets, "EsModuleTestFieldsWidget", 1);
        await AddWidgetAsync(page, addWidgetDropdown, widgets, "EsModuleTestFieldsWidget", 2);
        await AddWidgetAsync(page, addWidgetDropdown, widgets, "EsModuleTestHtmlBodyTrumbowyg", 3);
        await AddWidgetAsync(page, addWidgetDropdown, widgets, "EsModuleTestHtmlBodyWysiwyg", 4);
        await AddWidgetAsync(page, addWidgetDropdown, widgets, "EsModuleTestLiquidWidget", 5);

        var fieldsWidgets = WidgetOfType(page, "EsModuleTestFieldsWidget");
        await Assertions.Expect(fieldsWidgets).ToHaveCountAsync(2);

        for (var i = 0; i < 2; i++)
        {
            var instance = fieldsWidgets.Nth(i);
            await Assertions.Expect(TrumbowygBoxesOf(instance)).ToHaveCountAsync(2);
            await Assertions.Expect(CodeMirrorEditorsOf(instance)).ToHaveCountAsync(1);
            await Assertions.Expect(IconPickerItemsOf(instance)).Not.ToHaveCountAsync(0);

            // Freshly-added widgets have no seeded value, so the color field falls back to
            // its "#000000" default - both instances get this, which is exactly the point:
            // the second instance's module-scoped registration only ever runs once (per the
            // module-singleton hazard), so it can only have been initialized via the shared
            // observeAndInit() MutationObserver picking up its later AJAX insertion.
            await Assertions.Expect(ColorWrapperOf(instance)).ToHaveCSSAsync("background-color", "rgb(0, 0, 0)");
        }

        await Assertions.Expect(TrumbowygBoxesOf(WidgetOfType(page, "EsModuleTestHtmlBodyTrumbowyg"))).ToHaveCountAsync(1);
        await Assertions.Expect(TrumbowygBoxesOf(WidgetOfType(page, "EsModuleTestHtmlBodyWysiwyg"))).ToHaveCountAsync(1);
        await Assertions.Expect(CodeMirrorEditorsOf(WidgetOfType(page, "EsModuleTestLiquidWidget"))).ToHaveCountAsync(1);

        Assert.Empty(consoleErrors);

        await page.CloseAsync();
    }
}
