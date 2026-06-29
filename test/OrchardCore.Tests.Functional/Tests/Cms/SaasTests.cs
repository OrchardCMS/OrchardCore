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
        await page.GotoAndAssertOkAsync($"/{_fixture.Tenant.Prefix}");
        await Assertions.Expect(page.Locator("h4")).ToContainTextAsync("Welcome to Orchard Core, your site has been successfully set up.");
        await page.CloseAsync();
    }

    [Fact]
    public async Task SaasAdminLoginShouldWork()
    {
        var page = await _fixture.CreatePageAsync();
        await page.LoginAsync($"/{_fixture.Tenant.Prefix}");
        await page.GotoAndAssertOkAsync($"/{_fixture.Tenant.Prefix}/Admin");
        await Assertions.Expect(page.Locator(".menu-admin")).ToHaveAttributeAsync("id", "adminMenu");
        await page.CloseAsync();
    }

    [Fact]
    public async Task SaasAdminRootShouldNotKeepPreviousMenuItemActive()
    {
        var page = await _fixture.CreatePageAsync();
        await page.LoginAsync($"/{_fixture.Tenant.Prefix}");

        await page.GotoAndAssertOkAsync($"/{_fixture.Tenant.Prefix}/Admin/Features");

        var featuresLink = page.Locator("#adminMenu a[data-admin-hash][href*=\"/Admin/Features\"]").First;
        var activeFeaturesItem = featuresLink.Locator("xpath=ancestor::li[1][contains(concat(' ', normalize-space(@class), ' '), ' active ')]");

        await Assertions.Expect(activeFeaturesItem).ToHaveCountAsync(1);

        await page.GotoAndAssertOkAsync($"/{_fixture.Tenant.Prefix}/Admin");

        await Assertions.Expect(activeFeaturesItem).ToHaveCountAsync(0);

        await page.CloseAsync();
    }
}
