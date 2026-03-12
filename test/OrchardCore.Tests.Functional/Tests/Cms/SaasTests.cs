using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

[Collection(CmsTestCollection.Name)]
public sealed class SaasTests : IAsyncLifetime
{
    private readonly CmsSetupFixture _fixture;
    private TenantInfo _tenant;

    public SaasTests(CmsSetupFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync()
    {
        _tenant = TestUtils.GenerateTenantInfo("SaaS");
        var page = await _fixture.CreatePageAsync();
        await TenantHelper.NewTenantAsync(page, _tenant);
        await page.CloseAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task DisplaysTheHomePageOfTheSaasTheme()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync($"/{_tenant.Prefix}");
        await Assertions.Expect(page.Locator("h4")).ToContainTextAsync("Welcome to Orchard Core, your site has been successfully set up.");
        await page.CloseAsync();
    }

    [Fact]
    public async Task SaasAdminLoginShouldWork()
    {
        var page = await _fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{_tenant.Prefix}");
        await page.GotoAsync($"/{_tenant.Prefix}/Admin");
        await Assertions.Expect(page.Locator(".menu-admin")).ToHaveAttributeAsync("id", "adminMenu");
        await page.CloseAsync();
    }
}
