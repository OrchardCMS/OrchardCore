using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class HeadlessTests : CmsTestBase
{
    public HeadlessTests(CmsSetupFixture fixture) : base(fixture) { }

    protected override string RecipeName => "Headless";

    [Fact]
    public async Task DisplaysTheLoginScreenForTheHeadlessTheme()
    {
        var page = await Fixture.CreatePageAsync();
        await page.GotoAsync($"/{Tenant.Prefix}");
        await Assertions.Expect(page.Locator("h1")).ToContainTextAsync("Log in");
        await page.CloseAsync();
    }

    [Fact]
    public async Task HeadlessAdminLoginShouldWork()
    {
        var page = await Fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{Tenant.Prefix}");
        await page.GotoAsync($"/{Tenant.Prefix}/Admin");
        await Assertions.Expect(page.Locator(".menu-admin")).ToHaveAttributeAsync("id", "adminMenu");
        await page.CloseAsync();
    }
}
