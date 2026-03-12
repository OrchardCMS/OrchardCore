using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

[Collection(CmsTestCollection.Name)]
public sealed class ComingSoonTests : IAsyncLifetime
{
    private readonly CmsSetupFixture _fixture;
    private TenantInfo _tenant;

    public ComingSoonTests(CmsSetupFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync()
    {
        _tenant = TestUtils.GenerateTenantInfo("ComingSoon");
        var page = await _fixture.CreatePageAsync();
        await TenantHelper.NewTenantAsync(page, _tenant);
        await page.CloseAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task DisplaysTheHomePageOfTheComingSoonTheme()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync($"/{_tenant.Prefix}");
        await Assertions.Expect(page.Locator("h1")).ToContainTextAsync("Coming Soon");
        await Assertions.Expect(page.Locator("p")).ToContainTextAsync("We're working hard to finish the development of this site.");
        await page.CloseAsync();
    }

    [Fact]
    public async Task ComingSoonAdminLoginShouldWork()
    {
        var page = await _fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{_tenant.Prefix}");
        await page.GotoAsync($"/{_tenant.Prefix}/Admin");
        await Assertions.Expect(page.Locator(".menu-admin")).ToHaveAttributeAsync("id", "adminMenu");
        await page.CloseAsync();
    }
}
