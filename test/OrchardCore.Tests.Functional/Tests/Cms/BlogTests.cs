using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

[Collection(CmsTestCollection.Name)]
public sealed class BlogTests : IAsyncLifetime
{
    private readonly CmsSetupFixture _fixture;
    private TenantInfo _tenant;

    public BlogTests(CmsSetupFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync()
    {
        _tenant = TestUtils.GenerateTenantInfo("Blog");
        var page = await _fixture.CreatePageAsync();
        await TenantHelper.NewTenantAsync(page, _tenant);
        await page.CloseAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task DisplaysTheHomePageOfTheBlogRecipe()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync($"/{_tenant.Prefix}");
        await Assertions.Expect(page.Locator(".subheading")).ToContainTextAsync("This is the description of your blog");
        await page.CloseAsync();
    }

    [Fact]
    public async Task BlogAdminLoginShouldWork()
    {
        var page = await _fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{_tenant.Prefix}");
        await page.GotoAsync($"/{_tenant.Prefix}/Admin");
        await Assertions.Expect(page.Locator(".menu-admin")).ToHaveAttributeAsync("id", "adminMenu");
        await page.CloseAsync();
    }
}
