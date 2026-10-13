using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.DataProtection;
using OrchardCore.Entities;
using OrchardCore.Google;
using OrchardCore.Google.Authentication.Settings;
using OrchardCore.Tests.OptionsMonitoring;

namespace OrchardCore.Tests.Modules.OrchardCore.Google;

public class GoogleAuthenticationOptionsTests
{
    [Fact]
    public async Task UpdateSettings_ValidSettings_AddsSchemeWithoutReleasingTenant()
    {
        using var context = await TenantOptionsTestHelper.CreateSiteWithFeaturesAsync(GoogleConstants.Features.GoogleAuthentication);

        var shellContextTicks = await TenantOptionsTestHelper.GetShellContextTicksAsync(context);

        Assert.Null(await TenantOptionsTestHelper.GetSchemeAsync(context, GoogleDefaults.AuthenticationScheme));

        await UpdateSettingsAsync(context, "client-1");

        var scheme = await TenantOptionsTestHelper.GetSchemeAsync(context, GoogleDefaults.AuthenticationScheme);
        Assert.NotNull(scheme);
        Assert.Equal("Google", scheme.DisplayName);

        Assert.Contains("client_id=client-1", await TenantOptionsTestHelper.ChallengeAsync(context, GoogleDefaults.AuthenticationScheme));

        Assert.Equal(shellContextTicks, await TenantOptionsTestHelper.GetShellContextTicksAsync(context));
    }

    [Fact]
    public async Task UpdateSettings_ClientIdChanged_ChallengesWithNewClientIdWithoutReleasingTenant()
    {
        using var context = await TenantOptionsTestHelper.CreateSiteWithFeaturesAsync(GoogleConstants.Features.GoogleAuthentication);

        await UpdateSettingsAsync(context, "client-1");

        Assert.Contains("client_id=client-1", await TenantOptionsTestHelper.ChallengeAsync(context, GoogleDefaults.AuthenticationScheme));

        var shellContextTicks = await TenantOptionsTestHelper.GetShellContextTicksAsync(context);

        await UpdateSettingsAsync(context, "client-2");

        Assert.Equal("client-2", await TenantOptionsTestHelper.GetAsync(context, serviceProvider =>
            serviceProvider.GetRequiredService<IOptionsMonitor<GoogleOptions>>().Get(GoogleDefaults.AuthenticationScheme).ClientId));

        Assert.Contains("client_id=client-2", await TenantOptionsTestHelper.ChallengeAsync(context, GoogleDefaults.AuthenticationScheme));

        Assert.Equal(shellContextTicks, await TenantOptionsTestHelper.GetShellContextTicksAsync(context));
    }

    [Fact]
    public async Task UpdateSettings_ClientIdRemoved_RemovesSchemeWithoutReleasingTenant()
    {
        using var context = await TenantOptionsTestHelper.CreateSiteWithFeaturesAsync(GoogleConstants.Features.GoogleAuthentication);

        await UpdateSettingsAsync(context, "client-1");

        Assert.NotNull(await TenantOptionsTestHelper.GetSchemeAsync(context, GoogleDefaults.AuthenticationScheme));

        var shellContextTicks = await TenantOptionsTestHelper.GetShellContextTicksAsync(context);

        await UpdateSettingsAsync(context, clientId: null);

        Assert.Null(await TenantOptionsTestHelper.GetSchemeAsync(context, GoogleDefaults.AuthenticationScheme));
        Assert.Equal(shellContextTicks, await TenantOptionsTestHelper.GetShellContextTicksAsync(context));
    }

    private static Task UpdateSettingsAsync(Apis.Context.SiteContext context, string clientId)
        => TenantOptionsTestHelper.UpdateSiteAsync(
            context,
            (site, serviceProvider) =>
            {
                var protector = serviceProvider.GetRequiredService<IDataProtectionProvider>()
                    .CreateProtector(GoogleConstants.Features.GoogleAuthentication);

                site.Alter<GoogleAuthenticationSettings>(settings =>
                {
                    settings.ClientID = clientId;
                    settings.ClientSecret = protector.Protect("secret");
                });
            },
            notifier => notifier
                .RequestUpdate<GoogleAuthenticationSettings>()
                .RequestUpdate<GoogleOptions>(GoogleDefaults.AuthenticationScheme)
                .RequestUpdate<AuthenticationOptions>());
}
