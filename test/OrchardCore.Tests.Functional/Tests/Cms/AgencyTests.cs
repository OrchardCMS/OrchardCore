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
        var response = await page.GotoAsync("/");
        Assert.NotNull(response);
        Assert.True(response.Ok, $"Expected HTTP 200 but got {response.Status} for {response.Url}");
        await Assertions.Expect(page.Locator("#services")).ToContainTextAsync("Lorem ipsum dolor sit amet consectetur");
        await page.CloseAsync();
    }

    [Fact]
    public async Task AgencyAdminLoginShouldWork()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        var response = await page.GotoAsync("/Admin");
        Assert.NotNull(response);
        Assert.True(response.Ok, $"Expected HTTP 200 but got {response.Status} for {response.Url}");
        await Assertions.Expect(page.Locator(".menu-admin")).ToHaveAttributeAsync("id", "adminMenu");
        await page.CloseAsync();
    }
}
