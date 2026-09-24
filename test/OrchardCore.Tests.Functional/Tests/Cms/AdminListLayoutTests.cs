using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers the admin lists end to end: the columns each module declares with its IAdminListColumnProvider,
// the layout of the site, and the layout a user picks with the selector of a list. A list whose provider is
// not registered has no columns, so it falls back to the List layout and logs a warning, which the fixture
// reports once the tests ran.
public sealed class AdminListLayoutTests : CmsTestBase<AdminListLayoutTestsFixture>, IClassFixture<AdminListLayoutTestsFixture>
{
    // The lists of the features the Blog recipe enables.
    private static readonly string[] _lists =
    [
        "/Admin/Users/Index",
        "/Admin/Roles/Index",
        "/Admin/ContentTypes/List",
        "/Admin/ContentTypes/ListParts",
        "/Admin/Contents/ContentItems",
        "/Admin/AdminMenu/List",
        "/Admin/DeploymentPlan/Index",
        "/Admin/Deployment/RemoteInstance/Index",
        "/Admin/Deployment/RemoteClient/Index",
        "/Admin/indexing",
        "/Admin/Layers",
        "/Admin/MediaProfiles",
        "/Admin/Placements",
        "/Admin/Queries/Index",
        "/Admin/Recipes",
        "/Admin/Shortcodes",
        "/Admin/Templates",
        "/Admin/Features",
    ];

    public AdminListLayoutTests(AdminListLayoutTestsFixture fixture) : base(fixture) { }

    [Fact]
    public async Task LayoutSelector_UserPicksTable_ListRendersTheColumnsOfItsProviderAndKeepsTheChoice()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        await page.GotoAndAssertOkAsync("/Admin/Settings/admin");
        await page.GetByLabel("Let users choose the layout of a list").CheckAsync();
        await page.ClickSaveAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Asked for explicitly, so the list renders its rows whole whatever layout the other test gave the site.
        await page.GotoAndAssertOkAsync("/Admin/Users/Index?layout=List");
        await Assertions.Expect(page.Locator("ul.admin-list-list li.list-group-item:not(.text-bg-theme)", new() { HasText = "admin" }).First).ToBeVisibleAsync();

        var selector = page.Locator(".admin-list-layout-selector");
        await Assertions.Expect(selector).ToHaveCountAsync(1);

        await selector.Locator("a[title='Table']").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.Contains("layout=Table", page.Url);

        // The headers are the ones UsersAdminListColumnProvider declares, and the cells render the zones of the rows.
        Assert.Equal(["", "User", "Roles", "Actions"], await HeadersAsync(page));
        await Assertions.Expect(page.Locator(".admin-list-table tbody td.admin-list-column-user", new() { HasText = "admin" }).First).ToBeVisibleAsync();

        // The choice is kept for the list, so it opens the same way without the query string.
        await page.GotoAndAssertOkAsync("/Admin/Users/Index");
        await Assertions.Expect(page.Locator(".admin-list-table")).ToHaveCountAsync(1);

        // The features render one list per category and offer the layout once, beside their filters.
        await page.GotoAndAssertOkAsync("/Admin/Features");
        await Assertions.Expect(page.Locator(".admin-list-layout-selector")).ToHaveCountAsync(1);
        Assert.True(await page.Locator(".admin-list").CountAsync() > 1, "Expected the features to render one list per category.");

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task SiteLayout_Table_RendersEveryListWithTheColumnsOfItsProvider()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        await page.GotoAndAssertOkAsync("/Admin/Settings/admin");
        await page.GetByLabel("List layout", new() { Exact = true }).SelectOptionAsync(AdminListLayoutTestsFixture.Table);
        await page.ClickSaveAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        foreach (var url in _lists)
        {
            await page.GotoAndAssertOkAsync(url);

            // A list only renders as a table when a provider declared its columns. The headers are read rather
            // than seen: a list in a narrow column, e.g. the layers beside their zones, stacks its rows and hides them.
            var headers = await HeadersAsync(page);

            Assert.True(headers.Contains("Actions"), $"Expected {url} to render its list as a table, but its headers are [{string.Join(", ", headers)}].");
        }

        Assert.Empty(consoleErrors);
        await page.CloseAsync();
    }

    // The headers of the first table of the page.
    private static async Task<string[]> HeadersAsync(IPage page)
    {
        var headers = await page.Locator(".admin-list-table").First.Locator("thead th").AllTextContentsAsync();

        return headers.Select(header => header.Trim()).ToArray();
    }
}

public sealed class AdminListLayoutTestsFixture : CmsRecipeFixture
{
    public const string Table = "Table";

    protected override string RecipeName => "Blog";
}
