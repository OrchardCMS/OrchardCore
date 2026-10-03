using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

/// <summary>
/// Verifies the shape of 401 responses produced by the shared "Api" authentication scheme:
/// RFC 9110 §15.5.2 requires a WWW-Authenticate header on every 401 response, and an RFC 9457
/// Problem Details body is attached to it so JavaScript clients get a parseable payload,
/// whether the challenge comes from the scheme's own fallback or from the OpenIddict
/// validation handler.
/// </summary>
public sealed class ApiAuthenticationTests : CmsTestBase, IClassFixture<CmsSetupFixture>
{
    public ApiAuthenticationTests(CmsSetupFixture fixture) : base(fixture) { }

    protected override string RecipeName => "Blog";

    /// <summary>
    /// Even when no token scheme is registered (OpenID Token Validation disabled), the "Api"
    /// scheme's fallback 401 must carry a WWW-Authenticate header and a Problem Details body.
    /// These assertions also hold after the validation feature is enabled, where OpenIddict
    /// emits the header itself, so they are independent of test execution order in this class.
    /// </summary>
    [Fact]
    public async Task Api401AlwaysCarriesWwwAuthenticateHeaderAndProblemDetailsBody()
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

        Assert.True(response.Headers.TryGetValue("content-type", out var contentType));
        Assert.StartsWith("application/problem+json", contentType);
        Assert.Contains("\"status\":401", await response.TextAsync());

        await anonPage.CloseAsync();
    }

    /// <summary>
    /// With the OpenID validation feature enabled, an invalid bearer token must produce a 401
    /// carrying OpenIddict's RFC 6750 WWW-Authenticate header (error="invalid_token"), which is
    /// where the error details of a resource server challenge are conveyed, plus the RFC 9457
    /// Problem Details body attached to every "Api" scheme response.
    /// </summary>
    [Fact]
    public async Task OpenIddict401CarriesRfc6750HeaderAndProblemDetailsBody()
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
        Assert.StartsWith("Bearer", challenge);
        Assert.Contains("error=\"invalid_token\"", challenge);

        Assert.True(response.Headers.TryGetValue("content-type", out var contentType));
        Assert.StartsWith("application/problem+json", contentType);
        Assert.Contains("\"status\":401", await response.TextAsync());

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
