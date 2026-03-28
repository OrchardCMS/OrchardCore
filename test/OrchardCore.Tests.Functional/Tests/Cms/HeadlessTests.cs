using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class HeadlessTests : CmsTestBase<HeadlessFixture>, IClassFixture<HeadlessFixture>
{
    public HeadlessTests(HeadlessFixture fixture) : base(fixture) { }

    [Fact]
    public async Task DisplaysTheLoginScreenForTheHeadlessTheme()
    {
        var page = await Fixture.CreatePageAsync();
        await page.GotoAsync($"/{Fixture.Prefix}");
        await Assertions.Expect(page.Locator("h1")).ToContainTextAsync("Log in");
        await page.CloseAsync();
    }

    [Fact]
    public async Task HeadlessAdminLoginShouldWork()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync($"/{Fixture.Prefix}");
        await page.GotoAsync($"/{Fixture.Prefix}/Admin");
        await Assertions.Expect(page.Locator(".menu-admin")).ToHaveAttributeAsync("id", "adminMenu");
        await page.CloseAsync();
    }
}
