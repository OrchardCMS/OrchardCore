using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class AgencyTests : CmsTestBase
{
    public AgencyTests(CmsSetupFixture fixture) : base(fixture) { }

    protected override string RecipeName => "Agency";

    [Fact]
    public async Task DisplaysTheHomePageOfTheAgencyTheme()
    {
        var page = await Fixture.CreatePageAsync();
        await page.GotoAsync($"/{Tenant.Prefix}");
        await Assertions.Expect(page.Locator("#services")).ToContainTextAsync("Lorem ipsum dolor sit amet consectetur");
        await page.CloseAsync();
    }

    [Fact]
    public async Task AgencyAdminLoginShouldWork()
    {
        var page = await Fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{Tenant.Prefix}");
        await page.GotoAsync($"/{Tenant.Prefix}/Admin");
        await Assertions.Expect(page.Locator(".menu-admin")).ToHaveAttributeAsync("id", "adminMenu");
        await page.CloseAsync();
    }
}
