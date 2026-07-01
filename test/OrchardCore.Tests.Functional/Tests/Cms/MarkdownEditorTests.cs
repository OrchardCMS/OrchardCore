using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Smoke test for the EasyMDE WYSIWYG init script shared by Markdown's field
// and part editors (src/OrchardCore.Modules/OrchardCore.Markdown/Views/
// MarkdownField-Wysiwyg.Edit.cshtml and MarkdownBodyPart-Wysiwyg.Edit.cshtml -
// same jQuery-removal conversion, same EasyMDE init pattern). No shipped
// recipe configures a MarkdownField (field-level) with the Wysiwyg editor,
// but the Blog recipe's BlogPost content type uses MarkdownBodyPart with the
// Wysiwyg editor (Article, despite the similar name, uses HtmlBodyPart/
// Trumbowyg instead), which exercises the identical init code.
public sealed class MarkdownEditorTests : CmsTestBase<BlogFixture>, IClassFixture<BlogFixture>
{
    public MarkdownEditorTests(BlogFixture fixture) : base(fixture) { }

    [Fact]
    public async Task MarkdownEditor_Wysiwyg_InitializesEasyMde()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentItems/BlogPost");
        await page.Locator("li.list-group-item").Filter(new LocatorFilterOptions { HasText = "Man must explore" })
            .Locator("a.edit").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // EasyMDE replaces the plain <textarea> with its own toolbar and a
        // CodeMirror instance - their presence means the init script ran
        // without a JS error.
        await page.Locator(".editor-toolbar").WaitForAsync();
        await page.Locator(".CodeMirror").WaitForAsync();

        var editable = page.Locator(".CodeMirror textarea, .CodeMirror-code");
        Assert.True(await editable.CountAsync() > 0);

        await page.CloseAsync();
    }
}
