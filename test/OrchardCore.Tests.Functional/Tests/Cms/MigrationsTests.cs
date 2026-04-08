using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class MigrationsTests : CmsTestBase<MigrationsFixture>, IClassFixture<MigrationsFixture>
{
    public MigrationsTests(MigrationsFixture fixture) : base(fixture) { }

    [Fact]
    public async Task DisplaysTheHomePageOfTheMigrationsRecipe()
    {
        var page = await Fixture.CreatePageAsync();
        var response = await page.GotoAsync("/");
        Assert.NotNull(response);
        Assert.True(response.Ok, $"Expected HTTP 200 but got {response.Status} for {response.Url}");
        await Assertions.Expect(page.GetByText("Testing features having database migrations")).ToBeVisibleAsync();
        await page.CloseAsync();
    }

    [Fact]
    public async Task MigrationsAdminLoginShouldWork()
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
