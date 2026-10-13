using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using OrchardCore.Entities;
using OrchardCore.Microsoft.Authentication;
using OrchardCore.Microsoft.Authentication.Configuration;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.OptionsMonitoring;

namespace OrchardCore.Tests.Modules.OrchardCore.MicrosoftAuthentication;

public class MicrosoftAuthenticationOptionsTests
{
    [Fact]
    public async Task UpdateMicrosoftAccountSettings_AppIdChanged_UpdatesSchemeWithoutReleasingTenant()
    {
        using var context = await TenantOptionsTestHelper.CreateSiteWithFeaturesAsync(MicrosoftAuthenticationConstants.Features.MicrosoftAccount);

        var shellContextTicks = await TenantOptionsTestHelper.GetShellContextTicksAsync(context);

        Assert.Null(await TenantOptionsTestHelper.GetSchemeAsync(context, MicrosoftAccountDefaults.AuthenticationScheme));

        await UpdateMicrosoftAccountSettingsAsync(context, "app-1");

        Assert.NotNull(await TenantOptionsTestHelper.GetSchemeAsync(context, MicrosoftAccountDefaults.AuthenticationScheme));
        Assert.Contains("client_id=app-1", await TenantOptionsTestHelper.ChallengeAsync(context, MicrosoftAccountDefaults.AuthenticationScheme));

        await UpdateMicrosoftAccountSettingsAsync(context, "app-2");

        Assert.Contains("client_id=app-2", await TenantOptionsTestHelper.ChallengeAsync(context, MicrosoftAccountDefaults.AuthenticationScheme));
        Assert.Equal(shellContextTicks, await TenantOptionsTestHelper.GetShellContextTicksAsync(context));
    }

    [Fact]
    public async Task UpdateAzureADSettings_SettingsChanged_UpdatesSchemesWithoutReleasingTenant()
    {
        using var context = await TenantOptionsTestHelper.CreateSiteWithFeaturesAsync(MicrosoftAuthenticationConstants.Features.AAD);

        var shellContextTicks = await TenantOptionsTestHelper.GetShellContextTicksAsync(context);

        Assert.Null(await TenantOptionsTestHelper.GetSchemeAsync(context, Constants.AzureAd));

        await UpdateAzureADSettingsAsync(context, "app-1", "tenant-1", "Entra ID");

        Assert.Equal("Entra ID", (await TenantOptionsTestHelper.GetSchemeAsync(context, Constants.AzureAd)).DisplayName);
        Assert.NotNull(await TenantOptionsTestHelper.GetSchemeAsync(context, AzureADOptionsConfiguration.AzureAdOpenIdConnectScheme));

        var options = await GetOpenIdConnectOptionsAsync(context);
        Assert.Equal("app-1", options.ClientId);
        Assert.Equal("https://login.microsoftonline.com/tenant-1", options.Authority);

        await UpdateAzureADSettingsAsync(context, "app-2", "tenant-2", "Microsoft Entra ID");

        Assert.Equal("Microsoft Entra ID", (await TenantOptionsTestHelper.GetSchemeAsync(context, Constants.AzureAd)).DisplayName);

        options = await GetOpenIdConnectOptionsAsync(context);
        Assert.Equal("app-2", options.ClientId);
        Assert.Equal("https://login.microsoftonline.com/tenant-2", options.Authority);

        Assert.Equal(shellContextTicks, await TenantOptionsTestHelper.GetShellContextTicksAsync(context));
    }

    private static Task<OpenIdConnectOptions> GetOpenIdConnectOptionsAsync(SiteContext context)
        => TenantOptionsTestHelper.GetAsync(context, serviceProvider =>
            serviceProvider.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>().Get(AzureADOptionsConfiguration.AzureAdOpenIdConnectScheme));

    private static Task UpdateMicrosoftAccountSettingsAsync(SiteContext context, string appId)
        => TenantOptionsTestHelper.UpdateSiteAsync(
            context,
            (site, serviceProvider) =>
            {
                var protector = serviceProvider.GetRequiredService<IDataProtectionProvider>()
                    .CreateProtector(MicrosoftAuthenticationConstants.Features.MicrosoftAccount);

                site.Alter<MicrosoftAccountSettings>(settings =>
                {
                    settings.AppId = appId;
                    settings.AppSecret = protector.Protect("secret");
                });
            },
            notifier => notifier
                .RequestUpdate<MicrosoftAccountSettings>()
                .RequestUpdate<MicrosoftAccountOptions>(MicrosoftAccountDefaults.AuthenticationScheme)
                .RequestUpdate<AuthenticationOptions>());

    private static Task UpdateAzureADSettingsAsync(SiteContext context, string appId, string tenantId, string displayName)
        => TenantOptionsTestHelper.UpdateSiteAsync(
            context,
            (site, _) => site.Alter<AzureADSettings>(settings =>
            {
                settings.AppId = appId;
                settings.TenantId = tenantId;
                settings.DisplayName = displayName;
            }),
            notifier => notifier
                .RequestUpdate<AzureADSettings>()
                .RequestUpdate<MicrosoftIdentityOptions>(Constants.AzureAd)
                .RequestUpdate<OpenIdConnectOptions>(AzureADOptionsConfiguration.AzureAdOpenIdConnectScheme)
                .RequestUpdate<AuthenticationOptions>());
}
