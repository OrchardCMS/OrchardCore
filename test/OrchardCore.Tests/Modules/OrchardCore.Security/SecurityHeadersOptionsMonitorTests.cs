using OrchardCore.Security;
using OrchardCore.Security.Drivers;
using OrchardCore.Security.Options;
using OrchardCore.Security.ViewModels;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.Security;

public class SecurityHeadersOptionsMonitorTests
{
    [Fact]
    public async Task UpdateSecuritySettings_WithDriver_AppliesHeadersWithoutReleasingTenant()
    {
        using var context = new SiteContext()
            .WithRecipe("SaaS");

        await context.InitializeAsync();
        await context.EnableFeaturesAsync("OrchardCore.Security");

        var initialResponse = await context.Client.GetAsync(string.Empty, TestContext.Current.CancellationToken);

        Assert.Equal(SecurityHeaderDefaults.ReferrerPolicy, initialResponse.Headers.GetValues("Referrer-Policy").Single());

        var shellTicks = await context.GetShellTicksAsync();

        await context.UpdateSiteSettingsWithDriverAsync<SecuritySettingsDisplayDriver>("SecurityHeaders", model =>
        {
            var viewModel = Assert.IsType<SecuritySettingsViewModel>(model);
            viewModel.ContentSecurityPolicy = new Dictionary<string, string>
            {
                [ContentSecurityPolicyValue.FrameAncestors] = ContentSecurityPolicyOriginValue.Self,
            };
            viewModel.ReferrerPolicy = ReferrerPolicyValue.Origin;
        });

        Assert.Equal(shellTicks, await context.GetShellTicksAsync());

        var response = await context.Client.GetAsync(string.Empty, TestContext.Current.CancellationToken);

        Assert.Equal(ReferrerPolicyValue.Origin, response.Headers.GetValues("Referrer-Policy").Single());
        Assert.Equal("frame-ancestors 'self'", response.Headers.GetValues("Content-Security-Policy").Single());
    }
}
