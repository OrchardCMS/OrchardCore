using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using OrchardCore.Entities;
using OrchardCore.Facebook;
using OrchardCore.Facebook.Login.Settings;
using OrchardCore.Facebook.Settings;
using OrchardCore.ResourceManagement;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.OptionsMonitoring;

namespace OrchardCore.Tests.Modules.OrchardCore.Facebook;

public class FacebookOptionsTests
{
    [Fact]
    public async Task UpdateSettings_ValidSettings_AddsSchemeWithoutReleasingTenant()
    {
        using var context = await TenantOptionsTestHelper.CreateSiteWithFeaturesAsync(FacebookConstants.Features.Core, FacebookConstants.Features.Login);

        var shellContextTicks = await TenantOptionsTestHelper.GetShellContextTicksAsync(context);

        Assert.Null(await TenantOptionsTestHelper.GetSchemeAsync(context, FacebookDefaults.AuthenticationScheme));

        await UpdateSettingsAsync(context, "app-1");

        Assert.NotNull(await TenantOptionsTestHelper.GetSchemeAsync(context, FacebookDefaults.AuthenticationScheme));
        Assert.Contains("client_id=app-1", await TenantOptionsTestHelper.ChallengeAsync(context, FacebookDefaults.AuthenticationScheme));
        Assert.Equal(shellContextTicks, await TenantOptionsTestHelper.GetShellContextTicksAsync(context));
    }

    [Fact]
    public async Task UpdateSettings_AppIdChanged_UpdatesHandlerAndScriptsWithoutReleasingTenant()
    {
        using var context = await TenantOptionsTestHelper.CreateSiteWithFeaturesAsync(FacebookConstants.Features.Core, FacebookConstants.Features.Login);

        await UpdateSettingsAsync(context, "app-1");

        var scriptUrl = await GetFacebookScriptUrlAsync(context);

        Assert.Contains("client_id=app-1", await TenantOptionsTestHelper.ChallengeAsync(context, FacebookDefaults.AuthenticationScheme));

        var shellContextTicks = await TenantOptionsTestHelper.GetShellContextTicksAsync(context);

        await UpdateSettingsAsync(context, "app-2");

        Assert.Contains("client_id=app-2", await TenantOptionsTestHelper.ChallengeAsync(context, FacebookDefaults.AuthenticationScheme));
        Assert.NotEqual(scriptUrl, await GetFacebookScriptUrlAsync(context));
        Assert.Equal(shellContextTicks, await TenantOptionsTestHelper.GetShellContextTicksAsync(context));
    }

    [Fact]
    public async Task UpdateLoginSettings_CallbackPathChanged_UpdatesHandlerWithoutReleasingTenant()
    {
        using var context = await TenantOptionsTestHelper.CreateSiteWithFeaturesAsync(FacebookConstants.Features.Core, FacebookConstants.Features.Login);

        await UpdateSettingsAsync(context, "app-1");

        Assert.NotEqual("/signin-updated", await GetCallbackPathAsync(context));

        var shellContextTicks = await TenantOptionsTestHelper.GetShellContextTicksAsync(context);

        await TenantOptionsTestHelper.UpdateSiteAsync(
            context,
            (site, _) => site.Alter<FacebookLoginSettings>(settings => settings.CallbackPath = "/signin-updated"),
            notifier => notifier
                .RequestUpdate<FacebookOptions>(FacebookDefaults.AuthenticationScheme)
                .RequestUpdate<AuthenticationOptions>());

        Assert.Equal("/signin-updated", await GetCallbackPathAsync(context));
        Assert.Equal(shellContextTicks, await TenantOptionsTestHelper.GetShellContextTicksAsync(context));
    }

    private static Task<string> GetCallbackPathAsync(SiteContext context)
        => TenantOptionsTestHelper.GetAsync(context, serviceProvider =>
            serviceProvider.GetRequiredService<IOptionsMonitor<FacebookOptions>>().Get(FacebookDefaults.AuthenticationScheme).CallbackPath.Value);

    private static Task<string> GetFacebookScriptUrlAsync(SiteContext context)
        => TenantOptionsTestHelper.GetAsync(context, serviceProvider =>
            serviceProvider.GetRequiredService<IOptionsMonitor<ResourceManagementOptions>>().CurrentValue.ResourceManifests
                .SelectMany(manifest => manifest.GetResources("script").TryGetValue("fb", out var definitions) ? definitions : [])
                .Single()
                .Url);

    private static Task UpdateSettingsAsync(SiteContext context, string appId)
        => TenantOptionsTestHelper.UpdateSiteAsync(
            context,
            (site, serviceProvider) =>
            {
                var protector = serviceProvider.GetRequiredService<IDataProtectionProvider>()
                    .CreateProtector(FacebookConstants.Features.Core);

                site.Alter<FacebookSettings>(settings =>
                {
                    settings.AppId = appId;
                    settings.AppSecret = protector.Protect("secret");
                });
            },
            notifier => notifier
                .RequestUpdate<FacebookSettings>()
                .RequestUpdate<ResourceManagementOptions>()
                .RequestUpdate<FacebookOptions>(FacebookDefaults.AuthenticationScheme)
                .RequestUpdate<AuthenticationOptions>());
}
