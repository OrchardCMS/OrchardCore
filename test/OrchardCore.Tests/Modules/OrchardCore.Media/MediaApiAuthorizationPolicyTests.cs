using Microsoft.AspNetCore.Authentication;
using OrchardCore.Entities;
using OrchardCore.Media;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.OptionsMonitoring;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public class MediaApiAuthorizationPolicyTests
{
    [Fact]
    public async Task GetPolicyAsync_MediaApiPolicy_RequiresMediaApiSchemeAndAllowsCaching()
    {
        using var context = await TenantOptionsTestHelper.CreateSiteWithFeaturesAsync("OrchardCore.Media");

        Assert.Equal([MediaApiConstants.AuthenticationSchemeName], await GetPolicySchemesAsync(context, MediaApiConstants.AuthorizationPolicyName));
        Assert.True(await TenantOptionsTestHelper.GetAsync(context, serviceProvider =>
            serviceProvider.GetRequiredService<IAuthorizationPolicyProvider>().AllowsCachingPolicies));

        var scheme = await TenantOptionsTestHelper.GetSchemeAsync(context, MediaApiConstants.AuthenticationSchemeName);
        Assert.Equal(typeof(PolicySchemeHandler), scheme.HandlerType);
        Assert.Null(scheme.DisplayName);
    }

    [Fact]
    public async Task MediaApiScheme_NoSettings_ForwardsToCookieScheme()
    {
        using var context = await TenantOptionsTestHelper.CreateSiteWithFeaturesAsync("OrchardCore.Media");

        Assert.Equal(IdentityConstants.ApplicationScheme, await GetForwardedSchemeAsync(context));
    }

    [Fact]
    public async Task MediaApiEndpoint_AnonymousRequestWithCookieScheme_ChallengesCookieScheme()
    {
        using var context = await TenantOptionsTestHelper.CreateSiteWithFeaturesAsync("OrchardCore.Media");

        using var response = await context.Client.GetAsync("api/media/GetFolders", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("Login", response.Headers.Location.ToString());
    }

    [Fact]
    public async Task UpdateSettings_BearerScheme_ForwardsToApiSchemeWithoutReleasingTenant()
    {
        using var context = await TenantOptionsTestHelper.CreateSiteWithFeaturesAsync("OrchardCore.Media");

        Assert.Equal(IdentityConstants.ApplicationScheme, await GetForwardedSchemeAsync(context));

        var shellContextTicks = await TenantOptionsTestHelper.GetShellContextTicksAsync(context);

        await UpdateSchemeAsync(context, MediaApiAuthenticationScheme.Bearer);

        Assert.Equal(OrchardCoreConstants.AuthenticationSchemes.Api, await GetForwardedSchemeAsync(context));

        await UpdateSchemeAsync(context, MediaApiAuthenticationScheme.Cookie);

        Assert.Equal(IdentityConstants.ApplicationScheme, await GetForwardedSchemeAsync(context));
        Assert.Equal(shellContextTicks, await TenantOptionsTestHelper.GetShellContextTicksAsync(context));
    }

    [Fact]
    public async Task GetPolicyAsync_HubPolicy_ReturnsConfiguredPolicy()
    {
        using var context = await TenantOptionsTestHelper.CreateSiteWithFeaturesAsync("OrchardCore.Media");

        Assert.Equal(
            [OrchardCoreConstants.AuthenticationSchemes.Api, IdentityConstants.ApplicationScheme],
            await GetPolicySchemesAsync(context, MediaApiConstants.HubAuthorizationPolicyName));
    }

    private static Task UpdateSchemeAsync(SiteContext context, MediaApiAuthenticationScheme scheme)
        => TenantOptionsTestHelper.UpdateSiteAsync(
            context,
            (site, _) => site.Alter<MediaApiSettings>(settings => settings.AuthenticationScheme = scheme),
            _ => { });

    private static Task<string> GetForwardedSchemeAsync(SiteContext context)
        => TenantOptionsTestHelper.GetAsync(context, serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptionsMonitor<PolicySchemeOptions>>().Get(MediaApiConstants.AuthenticationSchemeName);
            var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;

            return options.ForwardDefaultSelector(httpContext);
        });

    private static Task<string[]> GetPolicySchemesAsync(SiteContext context, string policyName)
        => TenantOptionsTestHelper.GetAsync(context, async serviceProvider =>
        {
            var policy = await serviceProvider.GetRequiredService<IAuthorizationPolicyProvider>().GetPolicyAsync(policyName);

            return policy.AuthenticationSchemes.ToArray();
        });
}
