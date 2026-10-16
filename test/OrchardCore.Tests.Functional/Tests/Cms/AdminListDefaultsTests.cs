using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class AdminListDefaultsTests : CmsTestBase<AdminListDefaultsTestsFixture>, IClassFixture<AdminListDefaultsTestsFixture>
{
    public AdminListDefaultsTests(AdminListDefaultsTestsFixture fixture) : base(fixture) { }

    [Fact]
    public async Task LayoutDefaults_ConfigurationSiteAndBrowserPreferences_RespectPrecedenceAndIsolation()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var consoleErrors = page.CollectConsoleErrors();

        await page.GotoAndAssertOkAsync("/Admin/Users/Index");
        await Assertions.Expect(page.Locator(".admin-list-grid")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator(".admin-list-actions-menu")).ToHaveCountAsync(1);
        await page.Locator(".admin-list-actions-menu button").ClickAsync();
        await Assertions.Expect(page.Locator(".admin-list-actions-menu .dropdown-menu")).ToBeVisibleAsync();

        await page.GotoAndAssertOkAsync("/Admin/Settings/admin");
        var layoutSetting = page.GetByLabel("List layout", new() { Exact = true });
        await Assertions.Expect(layoutSetting).ToHaveValueAsync(string.Empty);
        Assert.Equal(["", "List", "Grid"], await layoutSetting.Locator("option").EvaluateAllAsync<string[]>(
            "options => options.map(option => option.value)"));

        await page.SetAdminListSettingsAsync("List", allowUserSelection: false, actionsLayout: "Buttons");
        await page.GotoAndAssertOkAsync("/Admin/Users/Index?layout=Grid");
        await Assertions.Expect(page.Locator(".admin-list-list")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator(".admin-list-actions-menu")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator(".admin-list-layout-selector")).ToHaveCountAsync(0);

        await page.SetAdminListSettingsAsync(string.Empty, allowUserSelection: false);
        await page.GotoAndAssertOkAsync("/Admin/Users/Index");
        await Assertions.Expect(page.Locator(".admin-list-grid")).ToHaveCountAsync(1);
        await page.GotoAndAssertOkAsync("/Admin/Settings/admin");
        await Assertions.Expect(layoutSetting).ToHaveValueAsync(string.Empty);

        await page.SetAdminListSettingsAsync("Grid", allowUserSelection: true, actionsLayout: "Menu");
        await page.GotoAndAssertOkAsync("/Admin/Users/Index");
        await page.Locator(".admin-list-layout-selector a[title='List']").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.GotoAndAssertOkAsync("/Admin/Users/Index");
        await Assertions.Expect(page.Locator(".admin-list-list")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator(".admin-list-actions-menu")).ToHaveCountAsync(1);

        await page.GotoAndAssertOkAsync("/Admin/Roles/Index");
        await Assertions.Expect(page.Locator(".admin-list-grid")).ToHaveCountAsync(1);

        var otherBrowser = await Fixture.CreatePageAsync();
        var otherErrors = otherBrowser.CollectConsoleErrors();
        await otherBrowser.LoginAsync();
        await otherBrowser.GotoAndAssertOkAsync("/Admin/Users/Index");
        await Assertions.Expect(otherBrowser.Locator(".admin-list-grid")).ToHaveCountAsync(1);
        await otherBrowser.CloseAsync();

        await page.SetAdminListSettingsAsync("List", allowUserSelection: true);
        await page.GotoAndAssertOkAsync("/Admin/Users/Index?layout=grid");
        await Assertions.Expect(page.Locator(".admin-list-grid")).ToHaveCountAsync(1);
        await page.GotoAndAssertOkAsync("/Admin/Users/Index?layout=NotAvailable");
        await Assertions.Expect(page.Locator(".admin-list-grid")).ToHaveCountAsync(1);
        await page.GotoAndAssertOkAsync("/Admin/Roles/Index");
        await Assertions.Expect(page.Locator(".admin-list-list")).ToHaveCountAsync(1);

        await page.SetAdminListSettingsAsync("List", allowUserSelection: false);
        await page.GotoAndAssertOkAsync("/Admin/Users/Index?layout=Grid");
        await Assertions.Expect(page.Locator(".admin-list-list")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator(".admin-list-layout-selector")).ToHaveCountAsync(0);

        Assert.Empty(consoleErrors);
        Assert.Empty(otherErrors);
        await page.CloseAsync();
    }
}

public sealed class AdminListDefaultsTestsFixture : CmsRecipeFixture
{
    public AdminListDefaultsTestsFixture()
        : base(builder =>
        {
            builder.Configuration["OrchardCore:Admin:List:DefaultLayout"] = "Grid";
            builder.Configuration["OrchardCore:Admin:List:DefaultActionsLayout"] = "Menu";
        })
    {
    }

    protected override string RecipeName => "Blog";
}
