using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class ComingSoonTests : CmsTestBase<ComingSoonFixture>, IClassFixture<ComingSoonFixture>
{
    public ComingSoonTests(ComingSoonFixture fixture) : base(fixture) { }

    [Fact]
    public async Task DisplaysTheHomePageOfTheComingSoonTheme()
    {
        var page = await Fixture.CreatePageAsync();
        await page.GotoAndAssertOkAsync("/");
        await Assertions.Expect(page.Locator("h1")).ToContainTextAsync("Coming Soon");
        await Assertions.Expect(page.Locator("p")).ToContainTextAsync("We're working hard to finish the development of this site.");
        await page.CloseAsync();
    }

    [Fact]
    public async Task ComingSoonAdminLoginShouldWork()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");
        await Assertions.Expect(page.Locator(".menu-admin")).ToHaveAttributeAsync("id", "adminMenu");
        await page.CloseAsync();
    }
}
