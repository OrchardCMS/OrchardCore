using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class SaasTests : CmsTestBase
{
    public SaasTests(CmsSetupFixture fixture) : base(fixture) { }

    protected override string RecipeName => "SaaS";

    [Fact]
    public async Task DisplaysTheHomePageOfTheSaasTheme()
    {
        var page = await Fixture.CreatePageAsync();
        await page.GotoAsync($"/{Tenant.Prefix}");
        await Assertions.Expect(page.Locator("h4")).ToContainTextAsync("Welcome to Orchard Core, your site has been successfully set up.");
        await page.CloseAsync();
    }

    [Fact]
    public async Task SaasAdminLoginShouldWork()
    {
        var page = await Fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{Tenant.Prefix}");
        await page.GotoAsync($"/{Tenant.Prefix}/Admin");
        await Assertions.Expect(page.Locator(".menu-admin")).ToHaveAttributeAsync("id", "adminMenu");
        await page.CloseAsync();
    }
}
