using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class SaasTests : IClassFixture<SaasFixture>, IAsyncLifetime
{
    private readonly SaasFixture _fixture;

    public SaasTests(SaasFixture fixture)
    {
        _fixture = fixture;
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
    {
        _fixture.AssertNoLoggedIssues();
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task DisplaysTheHomePageOfTheSaasTheme()
    {
        var page = await _fixture.CreatePageAsync();
        var response = await page.GotoAsync($"/{_fixture.Tenant.Prefix}");
        Assert.NotNull(response);
        Assert.True(response.Ok, $"Expected HTTP 200 but got {response.Status} for {response.Url}");
        await Assertions.Expect(page.Locator("h4")).ToContainTextAsync("Welcome to Orchard Core, your site has been successfully set up.");
        await page.CloseAsync();
    }

    [Fact]
    public async Task SaasAdminLoginShouldWork()
    {
        var page = await _fixture.CreatePageAsync();
        await page.LoginAsync($"/{_fixture.Tenant.Prefix}");
        var response = await page.GotoAsync($"/{_fixture.Tenant.Prefix}/Admin");
        Assert.NotNull(response);
        Assert.True(response.Ok, $"Expected HTTP 200 but got {response.Status} for {response.Url}");
        await Assertions.Expect(page.Locator(".menu-admin")).ToHaveAttributeAsync("id", "adminMenu");
        await page.CloseAsync();
    }
}
