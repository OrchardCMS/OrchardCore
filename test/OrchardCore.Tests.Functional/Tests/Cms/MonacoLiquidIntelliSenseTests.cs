using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers Monaco's liquid IntelliSense (src/OrchardCore.Modules/OrchardCore.Liquid/Assets/monaco/
// liquid-intellisense.ts's registerCompletionItemProvider("liquid", ...)) on the Template editor
// (src/OrchardCore.Modules/OrchardCore.Templates/Assets/ts/template-create.ts /
// template-edit.ts, both `monaco.editor.create(..., { language: "liquid" })`), the one shipped
// admin view that actually wires Monaco up with the "liquid" language id and depends-on=
// "liquid-intellisense monaco". No existing test exercises Monaco at all (EsModuleEditorTests
// only covers the CodeMirror/Trumbowyg/Wysiwyg/IconPicker editors), and Monaco's own suggestion
// widget requires actually driving its keyboard-triggered completion popup and reading its
// rendered rows - materially different from just checking initialization markers, so it needs
// its own dedicated coverage rather than folding into EsModuleEditorTests.
public sealed class MonacoLiquidIntelliSenseTests : CmsTestBase<BlogFixture>, IClassFixture<BlogFixture>
{
    public MonacoLiquidIntelliSenseTests(BlogFixture fixture) : base(fixture) { }

    // Monaco's own text input is a genuinely-focused, off-screen <textarea class="inputarea">
    // inside the editor's DOM - typing/pressing keys through Playwright's real keyboard API
    // (not FillAsync, which bypasses input events Monaco's own key bindings rely on) after
    // clicking the visible editor surface is the supported way to drive it.
    private static async Task TypeInEditorAsync(IPage page, ILocator editorRoot, string text)
    {
        await editorRoot.ClickAsync();
        await page.Keyboard.TypeAsync(text);
    }

    // Monaco renders the suggestion widget as a content widget inside the editor's own DOM
    // (widgetid="editor.widget.suggestWidget"), toggled via inline `display:block/none` rather
    // than a CSS class - it stays permanently mounted (hidden) between uses, so its mere DOM
    // presence proves nothing; only the visible display state does.
    private static ILocator SuggestWidget(IPage page)
        => page.Locator(".monaco-editor .suggest-widget[widgetid='editor.widget.suggestWidget']");

    private static ILocator SuggestWidgetRows(IPage page)
        => SuggestWidget(page).Locator(".monaco-list-row");

    [Fact]
    public async Task TemplateCreate_TypingOpenTag_SuggestsLiquidTags()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        await page.GotoAndAssertOkAsync("/Admin/Templates/Create");

        var editor = page.Locator(".template-editor");
        await Assertions.Expect(editor).ToBeVisibleAsync();

        // "{% " (the trailing space is liquid-intellisense.ts's own trigger character for tags)
        // should list a liquid tag keyword. Monaco's suggestion list is virtualized (only the
        // rows that fit the widget's fixed height actually render), and merges these with
        // suggestions from the HTML language service also registered against "liquid" - so the
        // combined list is alphabetized across both sources. "assign" is one of the few liquid
        // tags whose alphabetical position reliably lands within that first rendered page - "if"
        // and most other tags sort too far down to appear without scrolling first.
        await TypeInEditorAsync(page, editor, "{% ");

        await Assertions.Expect(SuggestWidget(page)).ToBeVisibleAsync();
        await Assertions.Expect(SuggestWidgetRows(page).GetByText("assign", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        await page.CloseAsync();
    }

    [Fact]
    public async Task TemplateCreate_TypingFilterPipe_SuggestsLiquidFilters()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        await page.GotoAndAssertOkAsync("/Admin/Templates/Create");

        var editor = page.Locator(".template-editor");
        await Assertions.Expect(editor).ToBeVisibleAsync();

        // Inside an object tag ("{{ ... }}"), a "|" followed by a space is liquid-intellisense.ts's
        // trigger for the filter list rather than the tag list (getLiquidContextInfo's inObject
        // branch). "append" is one of the few liquid filters whose alphabetical position reliably
        // lands within Monaco's first virtualized render page (see the tag-list test above for
        // why an early-alphabet item is required here rather than an arbitrary registered one).
        await TypeInEditorAsync(page, editor, "{{ Model.Title | ");

        await Assertions.Expect(SuggestWidget(page)).ToBeVisibleAsync();
        await Assertions.Expect(SuggestWidgetRows(page).GetByText("append", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        await page.CloseAsync();
    }
}
