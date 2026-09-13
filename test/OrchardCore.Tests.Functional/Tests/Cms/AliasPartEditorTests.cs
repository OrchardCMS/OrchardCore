using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers OrchardCore.Alias's alias-part-settings.ts: initializes a CodeMirror "liquid"
// pattern editor over the AliasPart type-part settings' Pattern textarea. This is a shared
// helper (liquid-pattern-editor.ts) used by several *Settings.Edit.cshtml pattern/template
// editors - Alias is the one named in the coverage issue.
//
// This test also guards against a real, previously-shipped, widespread regression found
// while writing it: every one of these scripts (14 files across 11 modules, e.g. Alias,
// ContentPreview, ContentFields, Autoroute, Title, Contents, Sms, Email, Tenants,
// RateLimits, Email.Smtp, Elasticsearch, ContentTypes, Notifications) was extracted from
// an inline Razor <script> block that used @Html.IdFor(x => x.Pattern) - which correctly
// resolves the DisplayDriver's generated, PREFIXED element id (e.g.
// "MyType_MyPart_MyDriver_Pattern") - into a standalone .ts file that instead hardcoded
// the literal, unprefixed field name (getElementById("Pattern")). Since the id is always
// prefixed by the owning DisplayDriver, none of these scripts ever found their target
// element - with zero console errors to reveal it. Fixed by switching every affected
// script to an attribute-suffix selector (e.g. "textarea[id$='Pattern']"), which matches
// regardless of the driver's prefix.
//
// NOTE: the plan doc's original objective for this task described a "Disabled"/dependent-
// field show-hide toggle (matching the Cors/Sitemaps pattern from earlier tasks) - that
// logic does NOT actually exist anywhere in OrchardCore.Alias (confirmed via search: no
// show/hide/toggle/collapse logic in the module at all). The real, only interactive script
// in this module is the CodeMirror pattern editor init covered here.
public sealed class AliasPartEditorTests : CmsTestBase<AliasPartEditorTestsFixture>, IClassFixture<AliasPartEditorTestsFixture>
{
    public AliasPartEditorTests(AliasPartEditorTestsFixture fixture) : base(fixture) { }

    [Fact]
    public async Task PatternEditor_InitializesCodeMirrorAndPersistsValue()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        await page.GotoAndAssertOkAsync("/Admin/ContentTypes/AliasEditorTestType/ContentParts/AliasPart/Edit");

        // The rendered id is prefixed by AliasPartSettingsDisplayDriver (e.g.
        // "AliasEditorTestType_AliasPart_AliasPartSettingsDisplayDriver_Pattern"), not a
        // plain "Pattern" - match by suffix.
        var patternTextarea = page.Locator("textarea[id$='Pattern']");
        await Assertions.Expect(patternTextarea).ToHaveCountAsync(1);

        // CodeMirror.fromTextArea() hides the original <textarea> and inserts a sibling
        // .CodeMirror wrapper element - its presence (and the textarea becoming hidden)
        // is the concrete, DOM-visible signature that initLiquidPatternEditor() actually ran.
        await Assertions.Expect(patternTextarea).ToBeHiddenAsync();
        var codeMirrorWrapper = page.Locator(".ocat-limited:has(textarea[id$='Pattern']) .CodeMirror");
        await Assertions.Expect(codeMirrorWrapper).ToHaveCountAsync(1);

        // Type into the visible CodeMirror editor surface (not the hidden textarea
        // directly) and confirm the change reaches the underlying form field, then save
        // and confirm it persisted server-side.
        var codeMirrorInput = codeMirrorWrapper.Locator(".CodeMirror-code");
        await codeMirrorInput.ClickAsync();
        await page.Keyboard.TypeAsync("{{ ContentItem.DisplayText | slugify }}");

        var textareaValue = await patternTextarea.InputValueAsync();
        Assert.Contains("slugify", textareaValue);

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save" }).First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GotoAndAssertOkAsync("/Admin/ContentTypes/AliasEditorTestType/ContentParts/AliasPart/Edit");
        var reloadedTextarea = page.Locator("textarea[id$='Pattern']");
        var reloadedValue = await reloadedTextarea.InputValueAsync();
        Assert.Contains("slugify", reloadedValue);

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }
}
