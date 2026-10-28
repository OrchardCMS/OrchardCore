using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

[Collection(CmsTestCollection.Name)]
public sealed class SaasTests : IAsyncLifetime
{
    private readonly SaasFixture _fixture;

    public SaasTests(SaasFixture fixture)
    {
        _fixture = fixture;
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
    {
        _fixture.AssertNoLoggedIssues();
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task DisplaysTheHomePageOfTheSaasTheme_Default_Succeeds()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAndAssertOkAsync($"/{_fixture.Tenant.Prefix}");
        await Assertions.Expect(page.Locator("h4")).ToContainTextAsync("Welcome to Orchard Core, your site has been successfully set up.");
        await page.CloseAsync();
    }

    [Fact]
    public async Task SaasAdminLogin_Default_Works()
    {
        var page = await _fixture.CreatePageAsync();
        await page.LoginAsync($"/{_fixture.Tenant.Prefix}");
        await page.GotoAndAssertOkAsync($"/{_fixture.Tenant.Prefix}/Admin");
        await Assertions.Expect(page.Locator(".menu-admin")).ToHaveAttributeAsync("id", "adminMenu");
        await page.CloseAsync();
    }

    [Fact]
    public async Task CreateTenant_DefaultTenantRecipes_AreNotListed()
    {
        var page = await _fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin/Tenants/Create");

        var recipes = page.Locator("#RecipeName");

        // The SaaS recipe is provided by the 'OrchardCore.Recipes.Default' feature, which is allowed on the Default tenant only.
        await Assertions.Expect(recipes.Locator("option[value=\"SaaS\"]")).ToHaveCountAsync(0);
        await Assertions.Expect(recipes.Locator("option[value=\"Blog\"]")).ToHaveCountAsync(1);
        await Assertions.Expect(recipes.Locator("option[value=\"SaasTenant\"]")).ToHaveCountAsync(1);

        await page.CloseAsync();
    }

    [Fact]
    public async Task VisitTenantSetup_DefaultTenantRecipes_AreNotListed()
    {
        var page = await _fixture.CreatePageAsync();

        // No recipe is preset, so the setup screen of the tenant lists the recipes it can be set up with.
        var tenant = TestUtils.GenerateTenantInfo(string.Empty);

        await page.LoginAsync();
        await page.CreateTenantAsync(tenant);
        await page.VisitTenantSetupPageAsync(tenant);

        await Assertions.Expect(page.Locator("#SiteName")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("[data-recipe-name=\"SaaS\"]")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("[data-recipe-name=\"Blog\"]")).ToHaveCountAsync(1);

        await page.CloseAsync();
    }

    [Fact]
    public async Task SaasAdminRoot_Default_NotKeepPreviousMenuItemActive()
    {
        var page = await _fixture.CreatePageAsync();
        await page.LoginAsync($"/{_fixture.Tenant.Prefix}");

        await page.GotoAndAssertOkAsync($"/{_fixture.Tenant.Prefix}/Admin/Features");

        var featuresLink = page.Locator("#adminMenu a[data-admin-hash][href*=\"/Admin/Features\"]").First;
        var activeFeaturesItem = featuresLink.Locator("xpath=ancestor::li[1][contains(concat(' ', normalize-space(@class), ' '), ' active ')]");

        await Assertions.Expect(activeFeaturesItem).ToHaveCountAsync(1);

        await page.GotoAndAssertOkAsync($"/{_fixture.Tenant.Prefix}/Admin");

        await Assertions.Expect(activeFeaturesItem).ToHaveCountAsync(0);

        await page.CloseAsync();
    }
}
