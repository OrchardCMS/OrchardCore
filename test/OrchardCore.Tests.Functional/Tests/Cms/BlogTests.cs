using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class BlogTests : CmsTestBase
{
    public BlogTests(CmsSetupFixture fixture) : base(fixture) { }

    protected override string RecipeName => "Blog";

    [Fact]
    public async Task DisplaysTheHomePageOfTheBlogRecipe()
    {
        var page = await Fixture.CreatePageAsync();
        await page.GotoAsync($"/{Tenant.Prefix}");
        await Assertions.Expect(page.Locator(".subheading")).ToContainTextAsync("This is the description of your blog");
        await page.CloseAsync();
    }

    [Fact]
    public async Task BlogAdminLoginShouldWork()
    {
        var page = await Fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{Tenant.Prefix}");
        await page.GotoAsync($"/{Tenant.Prefix}/Admin");
        await Assertions.Expect(page.Locator(".menu-admin")).ToHaveAttributeAsync("id", "adminMenu");
        await page.CloseAsync();
    }
}
