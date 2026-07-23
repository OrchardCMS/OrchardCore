using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class NavigationSelectionFixture : CmsRecipeFixture
{
    protected override string RecipeName => "NavigationSelection";
}

/// <summary>
/// Covers the case where the admin menu has both a content-type list link (which becomes the
/// declared owner of an item's edit page) and a link pointing directly at that edit page. The
/// declared owner is the default selection; a directly-clicked link is promoted over it.
/// </summary>
public sealed class NavigationSelectionTests : CmsTestBase<NavigationSelectionFixture>, IClassFixture<NavigationSelectionFixture>
{
    private const string ActiveAncestorLi = "xpath=ancestor::li[1][contains(concat(' ', normalize-space(@class), ' '), ' active ')]";

    public NavigationSelectionTests(NavigationSelectionFixture fixture) : base(fixture) { }

    [Fact]
    public async Task ClickingDirectLink_PromotesItOverDeclaredOwner()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");

        var directLink = page.GetByRole(AriaRole.Link, new() { Name = "Home Direct", Exact = true });
        var listLink = page.Locator("#adminMenu a[data-admin-hash][href=\"/Admin/Contents/ContentItems/Article\"]");

        await directLink.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.Contains("/Edit", page.Url, StringComparison.OrdinalIgnoreCase);

        // The clicked direct link wins over the declared owner (the Articles list).
        await Assertions.Expect(directLink.Locator(ActiveAncestorLi)).ToHaveCountAsync(1);
        await Assertions.Expect(listLink.Locator(ActiveAncestorLi)).ToHaveCountAsync(0);

        await page.CloseAsync();
    }

    [Fact]
    public async Task ArrivingWithoutClickingLink_KeepsDeclaredOwnerActive()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");

        // The direct link's href is the item's edit URL. Navigate straight to it without clicking a
        // nav link, simulating a bookmark or a content-list link: nothing was clicked, so the
        // declared owner (the Articles list) must stay active rather than the direct link.
        var directLink = page.GetByRole(AriaRole.Link, new() { Name = "Home Direct", Exact = true });
        var editUrl = await directLink.GetAttributeAsync("href");
        Assert.Contains("/Edit", editUrl, StringComparison.OrdinalIgnoreCase);

        await page.GotoAndAssertOkAsync(editUrl);

        var listLink = page.Locator("#adminMenu a[data-admin-hash][href=\"/Admin/Contents/ContentItems/Article\"]");
        var directLinkOnEdit = page.GetByRole(AriaRole.Link, new() { Name = "Home Direct", Exact = true });

        await Assertions.Expect(listLink.Locator(ActiveAncestorLi)).ToHaveCountAsync(1);
        await Assertions.Expect(directLinkOnEdit.Locator(ActiveAncestorLi)).ToHaveCountAsync(0);

        await page.CloseAsync();
    }
}
