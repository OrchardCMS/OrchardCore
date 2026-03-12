using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

[Collection(CmsTestCollection.Name)]
public sealed class MigrationsTests : IAsyncLifetime
{
    private readonly CmsSetupFixture _fixture;
    private TenantInfo _tenant;

    public MigrationsTests(CmsSetupFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync()
    {
        _tenant = TestUtils.GenerateTenantInfo("Migrations");
        var page = await _fixture.CreatePageAsync();
        await TenantHelper.NewTenantAsync(page, _tenant);
        await page.CloseAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task DisplaysTheHomePageOfTheMigrationsRecipe()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync($"/{_tenant.Prefix}");
        await Assertions.Expect(page.GetByText("Testing features having database migrations")).ToBeVisibleAsync();
        await page.CloseAsync();
    }

    [Fact]
    public async Task MigrationsAdminLoginShouldWork()
    {
        var page = await _fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{_tenant.Prefix}");
        await page.GotoAsync($"/{_tenant.Prefix}/Admin");
        await Assertions.Expect(page.Locator(".menu-admin")).ToHaveAttributeAsync("id", "adminMenu");
        await page.CloseAsync();
    }
}
