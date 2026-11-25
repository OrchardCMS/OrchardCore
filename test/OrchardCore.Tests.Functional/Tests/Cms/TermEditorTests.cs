using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers the editor of a taxonomy term, which saves the term in its taxonomy instead of publishing it, and warns when
// the taxonomy has unpublished changes. The site is set up with the Blog recipe ("Tags" ships published with the
// "Earth" and "Exploration" terms).
public sealed class TermEditorTests : CmsTestBase<TermEditorTestsFixture>, IClassFixture<TermEditorTestsFixture>
{
    private const string DraftWarning = "Visitors won't see your changes until the taxonomy is published.";

    public TermEditorTests(TermEditorTestsFixture fixture) : base(fixture) { }

    private static async Task OpenTagsTaxonomyAsync(IPage page)
    {
        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentItems/Taxonomy");
        var link = page.Locator("li.list-group-item").Filter(new LocatorFilterOptions { HasText = "Tags" }).Locator("a.edit").First;
        await page.GotoAndAssertOkAsync(await link.GetAttributeAsync("href"));
        await page.Locator("#menu").WaitForAsync();
    }

    private static async Task AssertTermEditorActionsAsync(IPage page)
    {
        var actionBar = page.Locator(".action-bar-bottom");
        await Assertions.Expect(actionBar.GetByRole(AriaRole.Button, new() { Name = "Save Term" })).ToBeVisibleAsync();
        await Assertions.Expect(actionBar.Locator("a.cancel")).ToBeVisibleAsync();
        await Assertions.Expect(actionBar.Locator("button.publish, button.draft, .btn-danger")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task TermEditor_SaveTerm_SavesInADraftOfTheTaxonomyAndWarnsAboutIt()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        await OpenTagsTaxonomyAsync(page);
        var taxonomyPath = new Uri(page.Url).AbsolutePath;

        // The taxonomy is published: a new term is saved, not published, and there is nothing to warn about.
        await page.GetByRole(AriaRole.Link, new() { Name = "Add Tag" }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await AssertTermEditorActionsAsync(page);
        await Assertions.Expect(page.GetByText(DraftWarning)).ToHaveCountAsync(0);

        await page.Locator("input[name='TitlePart.Title']").FillAsync("Mars");
        await page.Locator(".action-bar-bottom").GetByRole(AriaRole.Button, new() { Name = "Save Term" }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Back in the taxonomy editor, which lists the new term.
        await page.Locator("#menu").WaitForAsync();
        Assert.Equal(taxonomyPath, new Uri(page.Url).AbsolutePath);
        await Assertions.Expect(page.Locator("#menu li.menu-item").Filter(new LocatorFilterOptions { HasText = "Mars" })).ToHaveCountAsync(1);

        // Saving the term saved a draft of the taxonomy, which its terms now warn about until it is published.
        var row = page.Locator("#menu li.menu-item").Filter(new LocatorFilterOptions { HasText = "Earth" }).First;
        await row.GetByRole(AriaRole.Link, new() { Name = "Edit" }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await AssertTermEditorActionsAsync(page);
        await Assertions.Expect(page.GetByText(DraftWarning)).ToBeVisibleAsync();

        // Cancel goes back to the taxonomy.
        await page.Locator(".action-bar-bottom a.cancel").ClickAsync();
        await page.Locator("#menu").WaitForAsync();
        Assert.Equal(taxonomyPath, new Uri(page.Url).AbsolutePath);

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }
}

public sealed class TermEditorTestsFixture : CmsRecipeFixture
{
    protected override string RecipeName => "Blog";
}
