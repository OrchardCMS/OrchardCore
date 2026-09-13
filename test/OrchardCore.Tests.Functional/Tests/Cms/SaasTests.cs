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
        await Assertions.Expect(page.Locator("body")).ToHaveClassAsync("saas-page");
        await Assertions.Expect(page.Locator("h1")).ToContainTextAsync($"Welcome to {_fixture.Tenant.Name}");
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
    public async Task SaasRemoteManagementAuthenticationPages_Default_UseSaasTheme()
    {
        var page = await _fixture.CreatePageAsync();
        var prefix = $"/{_fixture.Tenant.Prefix}";

        await page.SetViewportSizeAsync(1440, 900);
        await page.GotoAndAssertOkAsync($"{prefix}/login");
        await Assertions.Expect(page.Locator("body")).ToHaveClassAsync("saas-auth-page");
        await Assertions.Expect(page.Locator(".saas-auth-header")).ToBeVisibleAsync();

        await page.LoginAsync(prefix);

        await page.SetViewportSizeAsync(390, 844);
        await page.GotoAndAssertOkAsync($"{prefix}/connect/verify");
        await Assertions.Expect(page.Locator("body")).ToHaveClassAsync("saas-auth-page");
        await Assertions.Expect(page.Locator("h1")).ToContainTextAsync("Connect a trusted device");
        await Assertions.Expect(page.Locator("#user_code")).ToBeVisibleAsync();

        const string codeChallenge = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFG";
        var authorizeUrl = $"{prefix}/connect/authorize"
            + "?client_id=orchardcore-cli"
            + "&response_type=code"
            + "&redirect_uri=http%3A%2F%2F127.0.0.1%2Fcallback"
            + "&scope=openid%20profile%20roles%20orchardcore.management"
            + $"&code_challenge={codeChallenge}"
            + "&code_challenge_method=S256";

        await page.GotoAndAssertOkAsync(authorizeUrl);
        await Assertions.Expect(page.Locator("h1")).ToContainTextAsync("Allow access to your tenant?");
        var allowAccess = page.GetByRole(AriaRole.Button, new() { Name = "Allow access" });
        await Assertions.Expect(allowAccess).ToBeVisibleAsync();
        await Assertions.Expect(allowAccess).ToHaveAttributeAsync("value", "yes");

        await page.GotoAndAssertOkAsync(
            $"{prefix}/connect/logout?client_id=orchardcore-cli&post_logout_redirect_uri=http%3A%2F%2F127.0.0.1%2F");
        await Assertions.Expect(page.Locator("h1")).ToContainTextAsync("Sign out of this tenant?");
        await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "Sign out" })).ToHaveAttributeAsync("value", "yes");

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
