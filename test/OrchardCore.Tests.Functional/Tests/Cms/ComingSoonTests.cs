using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class ComingSoonTests : CmsTestBase
{
    public ComingSoonTests(CmsSetupFixture fixture) : base(fixture) { }

    protected override string RecipeName => "ComingSoon";

    [Fact]
    public async Task DisplaysTheHomePageOfTheComingSoonTheme()
    {
        var page = await Fixture.CreatePageAsync();
        await page.GotoAsync($"/{Tenant.Prefix}");
        await Assertions.Expect(page.Locator("h1")).ToContainTextAsync("Coming Soon");
        await Assertions.Expect(page.Locator("p")).ToContainTextAsync("We're working hard to finish the development of this site.");
        await page.CloseAsync();
    }

    [Fact]
    public async Task ComingSoonAdminLoginShouldWork()
    {
        var page = await Fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{Tenant.Prefix}");
        await page.GotoAsync($"/{Tenant.Prefix}/Admin");
        await Assertions.Expect(page.Locator(".menu-admin")).ToHaveAttributeAsync("id", "adminMenu");
        await page.CloseAsync();
    }
}
