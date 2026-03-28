using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class AgencyTests : CmsTestBase<AgencyFixture>, IClassFixture<AgencyFixture>
{
    public AgencyTests(AgencyFixture fixture) : base(fixture) { }

    [Fact]
    public async Task DisplaysTheHomePageOfTheAgencyTheme()
    {
        var page = await Fixture.CreatePageAsync();
        await page.GotoAsync($"/{Fixture.Prefix}");
        await Assertions.Expect(page.Locator("#services")).ToContainTextAsync("Lorem ipsum dolor sit amet consectetur");
        await page.CloseAsync();
    }

    [Fact]
    public async Task AgencyAdminLoginShouldWork()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync($"/{Fixture.Prefix}");
        await page.GotoAsync($"/{Fixture.Prefix}/Admin");
        await Assertions.Expect(page.Locator(".menu-admin")).ToHaveAttributeAsync("id", "adminMenu");
        await page.CloseAsync();
    }
}
