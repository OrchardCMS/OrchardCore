using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class MigrationsTests : CmsTestBase
{
    public MigrationsTests(CmsSetupFixture fixture) : base(fixture) { }

    protected override string RecipeName => "Migrations";

    [Fact]
    public async Task DisplaysTheHomePageOfTheMigrationsRecipe()
    {
        var page = await Fixture.CreatePageAsync();
        await page.GotoAsync($"/{Tenant.Prefix}");
        await Assertions.Expect(page.GetByText("Testing features having database migrations")).ToBeVisibleAsync();
        await page.CloseAsync();
    }

    [Fact]
    public async Task MigrationsAdminLoginShouldWork()
    {
        var page = await Fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{Tenant.Prefix}");
        await page.GotoAsync($"/{Tenant.Prefix}/Admin");
        await Assertions.Expect(page.Locator(".menu-admin")).ToHaveAttributeAsync("id", "adminMenu");
        await page.CloseAsync();
    }
}
