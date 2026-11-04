using System.Text.Json;
using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

[Collection(CmsTestCollection.Name)]
public sealed class SaasTests : IAsyncLifetime
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
    public async Task DisplaysTheHomePageOfTheSaasTheme_Default_Succeeds()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAndAssertOkAsync($"/{_fixture.Tenant.Prefix}");
        await Assertions.Expect(page.Locator("h4")).ToContainTextAsync("Welcome to Orchard Core, your site has been successfully set up.");
        await page.CloseAsync();
    }

    [Fact]
    public async Task SaasRemoteManagementBootstrap_Default_UsesTenantAuthority()
    {
        var page = await _fixture.CreatePageAsync();
        var tenantUrl = $"{_fixture.BaseUrl.TrimEnd('/')}/{_fixture.Tenant.Prefix}/";

        var bootstrapResponse = await page.APIRequest.GetAsync(
            $"{tenantUrl}.well-known/orchardcore-management");

        Assert.True(bootstrapResponse.Ok);
        using var bootstrap = JsonDocument.Parse(await bootstrapResponse.TextAsync());
        var authentication = bootstrap.RootElement.GetProperty("authentication");

        Assert.Equal(tenantUrl, authentication.GetProperty("authority").GetString());
        Assert.Equal("orchardcore-cli", authentication.GetProperty("clientId").GetString());
        Assert.Contains("authorization_code", authentication.GetProperty("grantTypes").EnumerateArray().Select(value => value.GetString()));
        Assert.Contains("urn:ietf:params:oauth:grant-type:device_code", authentication.GetProperty("grantTypes").EnumerateArray().Select(value => value.GetString()));
        Assert.Contains("orchardcore.management", authentication.GetProperty("scopes").EnumerateArray().Select(value => value.GetString()));

        var discoveryResponse = await page.APIRequest.GetAsync(
            $"{tenantUrl}.well-known/openid-configuration");

        Assert.True(discoveryResponse.Ok);
        using var discovery = JsonDocument.Parse(await discoveryResponse.TextAsync());
        Assert.Equal(tenantUrl.TrimEnd('/'), discovery.RootElement.GetProperty("issuer").GetString());
        Assert.Equal($"{tenantUrl}connect/authorize", discovery.RootElement.GetProperty("authorization_endpoint").GetString());
        Assert.Equal($"{tenantUrl}connect/device", discovery.RootElement.GetProperty("device_authorization_endpoint").GetString());

        await page.CloseAsync();
    }

    [Fact]
    public async Task SaasAdminLogin_Default_Works()
    {
        var page = await _fixture.CreatePageAsync();
        await page.LoginAsync($"/{_fixture.Tenant.Prefix}");
        await page.GotoAndAssertOkAsync($"/{_fixture.Tenant.Prefix}/Admin");
        await Assertions.Expect(page.Locator(".menu-admin")).ToHaveAttributeAsync("id", "adminMenu");
        await page.CloseAsync();
    }

    [Fact]
    public async Task SaasAdminRoot_Default_NotKeepPreviousMenuItemActive()
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
