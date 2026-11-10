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
        // its own backdrop. No shortcode descriptors are registered in this test recipe, so
        // click the modal's own Cancel button instead of a shortcode card - same principle,
        // no seeded shortcode data required.
        var cancelButton = modal.Locator(".modal-footer button", new LocatorLocatorOptions { HasText = "Cancel" });
        await Assertions.Expect(cancelButton).ToBeVisibleAsync();
        await cancelButton.ClickAsync();

        await Assertions.Expect(modal).Not.ToBeVisibleAsync();

        await page.CloseAsync();
    }
}
