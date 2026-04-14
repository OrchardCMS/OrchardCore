using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class BlogTests : CmsTestBase<BlogFixture>, IClassFixture<BlogFixture>
{
    public BlogTests(BlogFixture fixture) : base(fixture) { }

    [Fact]
    public async Task DisplaysTheHomePageOfTheBlogRecipe()
    {
        var page = await Fixture.CreatePageAsync();
        await page.GotoAndAssertOkAsync("/");
        await Assertions.Expect(page.Locator(".subheading")).ToContainTextAsync("This is the description of your blog");
        await page.CloseAsync();
    }

    [Fact]
    public async Task BlogAdminLoginShouldWork()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");
        await Assertions.Expect(page.Locator(".menu-admin")).ToHaveAttributeAsync("id", "adminMenu");
        await page.CloseAsync();
    }

    [Fact]
    public async Task BlogAdminNavigationShouldNotUseAdminQueryParameter()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();
        await page.GotoAndAssertOkAsync("/Admin");

        var adminLink = page.Locator("#adminMenu a[data-admin-hash][href^=\"/\"]").First;
        await adminLink.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.DoesNotContain("?admin=", page.Url, StringComparison.OrdinalIgnoreCase);

        var prefsCookie = (await page.Context.CookiesAsync())
            .FirstOrDefault(c => c.Name.EndsWith("-adminPreferences", StringComparison.Ordinal));
        Assert.NotNull(prefsCookie);

        var decodedPrefs = Uri.UnescapeDataString(prefsCookie.Value);
        Assert.Contains("selectedNavHash", decodedPrefs, StringComparison.Ordinal);

        await page.CloseAsync();
    }
}
