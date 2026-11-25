using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// REAL BUG (found via user report, not from writing this test first): clicking the "[/]"
// insert-shortcode button on a Trumbowyg/Wysiwyg HtmlBodyPart editor showed the modal's
// backdrop but never the modal itself, leaving the entire page unclickable (the backdrop
// intercepts every click with no visible dialog to interact with or dismiss).
//
// Root cause: OrchardCore.Shortcodes' Assets/js/shortcodes.js (a global classic script, not
// migrated to a Vue SFC) creates its shortcode-picker Vue app via
// `Vue.createApp({...}).mount(element)`, where `element` is the actual
// #shortcodeModal div (the ".modal" itself). Vue 2's equivalent (`new Vue({...}).$mount(el)`)
// REPLACES `el` with the rendered template, so `this.$el` used to BE that same ".modal" div.
// Vue 3's mount() instead renders the app's template INSIDE `element`, so after the Vue2->
// Vue3 migration `this.$el` became the modal's first rendered child (".modal-dialog") instead
// of the ".modal" container itself. `init()` called `new bootstrap.Modal(this.$el)`, so
// Bootstrap's "show"/"display:block" classes landed on .modal-dialog while .modal (and its
// backdrop) stayed exactly as Bootstrap's CSS renders an inert, display:none modal shell with
// its backdrop already showing - the backdrop is real and undismissable, but the dialog itself
// is invisible. Fixed by targeting `element` (the actual .modal container, closed over from
// initializeShortcodesApp's outer parameter) instead of `this.$el`.
public sealed class ShortcodeModalTests : CmsTestBase<EsModuleTestsFixture>, IClassFixture<EsModuleTestsFixture>
{
    public ShortcodeModalTests(EsModuleTestsFixture fixture) : base(fixture) { }

    [Fact]
    public async Task ShortcodeButton_Click_ShowsVisibleInteractiveModal()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentTypes/EsModuleTestPage/Create");
        await page.Locator("#TitlePart_Title").FillAsync("Shortcode Modal Test Page");

        var placeholder = page.Locator(".widget-template-placeholder-flowpart");
        await placeholder.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });

        var addWidgetDropdown = page.Locator(".btn-widget-add-below .dropdown-toggle");
        await addWidgetDropdown.ClickAsync();
        await page.Locator(".dropdown-item.add-widget[data-widget-type='EsModuleTestHtmlBodyWysiwyg']").ClickAsync();

        var widget = page.Locator(".widget.widget-editor.card[data-content-type='EsModuleTestHtmlBodyWysiwyg']");
        await Assertions.Expect(widget).ToHaveCountAsync(1);
        await Assertions.Expect(widget.Locator(".trumbowyg-box")).ToHaveCountAsync(1);

        // Trumbowyg's own insert-shortcode toolbar button (added by
        // trumbowyg.shortcodes.js's insertShortcode plugin). Only the "Wysiwyg" variant gets
        // this button by default (extendDefaultButtonsWithShortcode() in the shared
        // trumbowyg-editor.ts component) - the plain "Trumbowyg" variant only has it when an
        // admin explicitly adds "insertShortcode" to its configured button list.
        var shortcodeButton = widget.Locator("button.trumbowyg-insertShortcode-button");
        await shortcodeButton.ClickAsync();

        var modal = page.Locator("#shortcodeModal");
        // The backdrop shows regardless of the bug (Bootstrap creates it independently of
        // the .modal element's own show/hide state) - the real assertion is that the modal
        // itself becomes visible, not just its backdrop.
        await Assertions.Expect(page.Locator(".modal-backdrop")).ToBeVisibleAsync();
        await Assertions.Expect(modal).ToBeVisibleAsync();
        await Assertions.Expect(modal).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("\\bshow\\b"));

        // The modal being merely "visible" per Playwright's CSS-based definition isn't
        // enough on its own to prove the bug is fixed (Playwright's visibility check doesn't
        // care that a z-index'd backdrop sits on top) - clicking through to an element inside
        // the modal only succeeds if the modal is genuinely interactive, i.e. not obscured by
        // its own backdrop. Click the modal's own Cancel button rather than a shortcode card -
        // same principle, without depending on the seeded shortcode template.
        var cancelButton = modal.Locator(".modal-footer button", new LocatorLocatorOptions { HasText = "Cancel" });
        await Assertions.Expect(cancelButton).ToBeVisibleAsync();
        await cancelButton.ClickAsync();

        await Assertions.Expect(modal).Not.ToBeVisibleAsync();

        await page.CloseAsync();
    }

    // REAL BUG (#20312): on a full page load of an editor using the default CodeMirror
    // HtmlField/HtmlBodyPart editor, the insert-shortcode button got two click handlers.
    // html-field.ts/html-body-part.ts are ES modules, so they run (and call
    // initializeCodeMirrorShortcodeWrapper, which creates the button and binds its handler)
    // before DOMContentLoaded. shortcodes.js's DOMContentLoaded handler then queried every
    // .shortcode-modal-btn on the page and bound a second handler that looked for a
    // .shortcode-modal-input in the CodeMirror wrapper, found none, and passed null to
    // insertAtCaret ("Cannot read properties of null (reading 'selectionStart')"). Both
    // handlers also created their own bootstrap.Modal on the same element, so every open left
    // an extra backdrop behind that blocked the whole page after Cancel.
    [Fact]
    public async Task CodeMirrorShortcodeButton_FullPageLoad_InsertsShortcodeAndLeavesNoBackdrop()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        var consoleErrors = page.CollectConsoleErrors();

        // The seeded page already has the fields widget (with a default CodeMirror HtmlField)
        // in its FlowPart, so this is a genuine full page load, not an AJAX-added widget.
        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentItems/EsModuleTestPage");
        await page.Locator("li.list-group-item").Filter(new LocatorFilterOptions { HasText = "ES Module Test Page" })
            .Locator("a.edit").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var widget = page.Locator(".widget.widget-editor.card[data-content-type='EsModuleTestFieldsWidget']");
        var codeMirrorWrapper = widget.Locator(".shortcode-modal-wrapper:has(.CodeMirror)");
        await Assertions.Expect(codeMirrorWrapper).ToHaveCountAsync(1);

        var shortcodeButton = codeMirrorWrapper.Locator(".shortcode-modal-btn");
        var modal = page.Locator("#shortcodeModal");
        var backdrops = page.Locator(".modal-backdrop");

        // Open and cancel twice: each open must show exactly one backdrop, and closing must
        // remove it, also on the second open (a new bootstrap.Modal per open used to stack).
        for (var i = 0; i < 2; i++)
        {
            await shortcodeButton.ClickAsync();
            await Assertions.Expect(modal).ToBeVisibleAsync();
            await Assertions.Expect(backdrops).ToHaveCountAsync(1);

            await modal.Locator(".modal-footer button", new LocatorLocatorOptions { HasText = "Cancel" }).ClickAsync();
            await Assertions.Expect(modal).Not.ToBeVisibleAsync();
            await Assertions.Expect(backdrops).ToHaveCountAsync(0);
        }

        // Insert the shortcode seeded by the recipe's ShortcodeTemplates step.
        await shortcodeButton.ClickAsync();
        await Assertions.Expect(modal).ToBeVisibleAsync();
        await modal.Locator("button[data-default-value='[es_test]']").ClickAsync();
        await Assertions.Expect(modal).Not.ToBeVisibleAsync();
        await Assertions.Expect(backdrops).ToHaveCountAsync(0);

        var editorValue = await codeMirrorWrapper.Locator(".CodeMirror")
            .EvaluateAsync<string>("element => element.CodeMirror.getValue()");
        Assert.Contains("[es_test]", editorValue);

        Assert.Empty(consoleErrors);

        await page.CloseAsync();
    }
}
