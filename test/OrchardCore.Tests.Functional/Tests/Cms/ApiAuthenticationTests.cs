using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

/// <summary>
/// Verifies the shape of 401 responses produced by the shared "Api" authentication scheme:
/// RFC 9110 §15.5.2 requires a WWW-Authenticate header on every 401 response, and when the
/// OpenIddict validation handler issues the challenge, an RFC 9457 Problem Details body is
/// attached alongside the RFC 6750 header so JavaScript clients get a parseable payload.
/// </summary>
public sealed class ApiAuthenticationTests : CmsTestBase, IClassFixture<CmsSetupFixture>
{
    public ApiAuthenticationTests(CmsSetupFixture fixture) : base(fixture) { }

    protected override string RecipeName => "Blog";

    /// <summary>
    /// Even when no token scheme is registered (OpenID Token Validation disabled), the "Api"
    /// scheme's fallback 401 must carry a WWW-Authenticate header. This assertion also holds
    /// after the validation feature is enabled, where OpenIddict emits the header itself, so
    /// it is independent of test execution order within this class.
    /// </summary>
    [Fact]
    public async Task Api401AlwaysCarriesWwwAuthenticateHeader()
    {
        var page = await Fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{Tenant.Prefix}");
        await page.CloseAsync();

        var anonPage = await Fixture.CreatePageAsync();
        var response = await anonPage.APIRequest.GetAsync(
            $"{Fixture.BaseUrl}/{Tenant.Prefix}/api/content/does-not-exist",
            new APIRequestContextOptions { MaxRedirects = 0 });

        Assert.Equal(401, response.Status);
        Assert.True(response.Headers.TryGetValue("www-authenticate", out var challenge));
        Assert.StartsWith("Bearer", challenge);

        await anonPage.CloseAsync();
    }

    /// <summary>
    /// With the OpenID validation feature enabled, an invalid bearer token must produce a 401
    /// with OpenIddict's RFC 6750 WWW-Authenticate header (error="invalid_token") and an
    /// RFC 9457 Problem Details body mirroring the error code and description.
    /// </summary>
    [Fact]
    public async Task OpenIddict401IncludesProblemDetailsBody()
    {
        var page = await Fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, $"/{Tenant.Prefix}");
        await ConfigureOpenIdServerAsync(page, $"/{Tenant.Prefix}");
        await page.CloseAsync();

        var anonPage = await Fixture.CreatePageAsync();
        var response = await anonPage.APIRequest.GetAsync(
            $"{Fixture.BaseUrl}/{Tenant.Prefix}/api/content/does-not-exist",
            new APIRequestContextOptions
            {
                MaxRedirects = 0,
                Headers = new Dictionary<string, string> { ["Authorization"] = "Bearer invalid-token" },
            });

        Assert.Equal(401, response.Status);

        Assert.True(response.Headers.TryGetValue("www-authenticate", out var challenge));
        Assert.Contains("error=\"invalid_token\"", challenge);

        Assert.True(response.Headers.TryGetValue("content-type", out var contentType));
        Assert.StartsWith("application/problem+json", contentType);

        var body = await response.TextAsync();
        Assert.Contains("\"status\":401", body);
        Assert.Contains("invalid_token", body);

        await anonPage.CloseAsync();
    }

    private static async Task ConfigureOpenIdServerAsync(IPage page, string prefix)
    {
        await FeatureHelper.EnableFeatureAsync(page, prefix, "OrchardCore.OpenId.Server");
        await FeatureHelper.EnableFeatureAsync(page, prefix, "OrchardCore.OpenId.Validation");

        await page.GotoAsync($"{prefix}/Admin/OpenId/ServerConfiguration");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var enableTokenEndpoint = page.Locator("label:has-text('Enable Token Endpoint')").Locator("..").Locator("input[type='checkbox']");
        await enableTokenEndpoint.WaitForAsync(new() { State = WaitForSelectorState.Attached, Timeout = 15000 });
        if (!await enableTokenEndpoint.IsCheckedAsync())
        {
            await enableTokenEndpoint.CheckAsync();
        }

        await page.Locator("button.btn-primary[type='submit']").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
