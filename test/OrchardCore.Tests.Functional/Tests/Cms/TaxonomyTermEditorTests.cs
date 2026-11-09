using System.Text.RegularExpressions;
using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class TaxonomyTermEditorTests : CmsTestBase<BlogFixture>, IClassFixture<BlogFixture>
{
    public TaxonomyTermEditorTests(BlogFixture fixture) : base(fixture) { }

    private static async Task OpenTermAsync(IPage page, string termName)
    {
        await page.Locator("#menu li.menu-item").Filter(new LocatorFilterOptions { HasText = termName })
            .Locator("a.btn-success").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private static async Task PublishAsync(IPage page)
    {
        await page.Locator(".btn.publish").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.Locator("#menu").WaitForAsync();
    }

    [Fact]
    public async Task TermEditor_EmptyPermalink_GeneratesItOnCreateAndEdit()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentItems/Taxonomy");
        await page.Locator("li.list-group-item").Filter(new LocatorFilterOptions { HasText = "Tags" })
            .Locator("a.edit").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var taxonomyId = Regex.Match(page.Url, "/ContentItems/([^/]+)/Edit").Groups[1].Value;

        await page.GotoAndAssertOkAsync($"/Admin/Taxonomies/Create/Tag?taxonomyContentItemId={taxonomyId}");
        await page.Locator("#TitlePart_Title").FillAsync("Mars");
        await PublishAsync(page);

        await OpenTermAsync(page, "Mars");
        var permalink = page.Locator("#AutoroutePart_Path");
        await Assertions.Expect(permalink).ToHaveValueAsync("mars");

        await permalink.FillAsync(string.Empty);
        await PublishAsync(page);

        await OpenTermAsync(page, "Mars");
        await Assertions.Expect(page.Locator("#AutoroutePart_Path")).ToHaveValueAsync("mars");

        await page.CloseAsync();
    }
}
