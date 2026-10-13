using OrchardCore.Entities;
using OrchardCore.Media;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.OptionsMonitoring;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public class MediaApiAuthorizationPolicyTests
{
    [Fact]
    public async Task UpdateSettings_BearerScheme_RequiresApiSchemeWithoutReleasingTenant()
    {
        using var context = await TenantOptionsTestHelper.CreateSiteWithFeaturesAsync("OrchardCore.Media");

        Assert.Equal([IdentityConstants.ApplicationScheme], await GetPolicySchemesAsync(context, MediaApiConstants.AuthorizationPolicyName));

        var shellContextTicks = await TenantOptionsTestHelper.GetShellContextTicksAsync(context);

        await TenantOptionsTestHelper.UpdateSiteAsync(
            context,
            (site, _) => site.Alter<MediaApiSettings>(settings => settings.AuthenticationScheme = MediaApiAuthenticationScheme.Bearer),
            _ => { });

        Assert.Equal([OrchardCoreConstants.AuthenticationSchemes.Api], await GetPolicySchemesAsync(context, MediaApiConstants.AuthorizationPolicyName));
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

    private static Task<string[]> GetPolicySchemesAsync(SiteContext context, string policyName)
        => TenantOptionsTestHelper.GetAsync(context, async serviceProvider =>
        {
            var policy = await serviceProvider.GetRequiredService<IAuthorizationPolicyProvider>().GetPolicyAsync(policyName);

            return policy.AuthenticationSchemes.ToArray();
        });
}
