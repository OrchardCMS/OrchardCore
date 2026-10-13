using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using OpenIddict.Server;
using OpenIddict.Server.DataProtection;
using OpenIddict.Validation;
using OpenIddict.Validation.AspNetCore;
using OrchardCore.Entities;
using OrchardCore.OpenId;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.OptionsMonitoring;

namespace OrchardCore.Tests.Modules.OrchardCore.OpenId;

public class OpenIdOptionsTests
{
    [Fact]
    public async Task UpdateClientSettings_AuthorityChanged_UpdatesHandlerWithoutReleasingTenant()
    {
        using var context = await TenantOptionsTestHelper.CreateSiteWithFeaturesAsync(OpenIdConstants.Features.Client);

        var shellContextTicks = await TenantOptionsTestHelper.GetShellContextTicksAsync(context);

        Assert.Null(await TenantOptionsTestHelper.GetSchemeAsync(context, OpenIdConnectDefaults.AuthenticationScheme));

        await UpdateClientSettingsAsync(context, "https://idp-1.example.com/", "Identity Provider");

        Assert.Equal("Identity Provider", (await TenantOptionsTestHelper.GetSchemeAsync(context, OpenIdConnectDefaults.AuthenticationScheme)).DisplayName);

        var options = await GetOpenIdConnectOptionsAsync(context);
        var configurationManager = options.ConfigurationManager;

        Assert.Equal("https://idp-1.example.com/", options.Authority);
        Assert.NotNull(configurationManager);

        await UpdateClientSettingsAsync(context, "https://idp-2.example.com/", "Other Identity Provider");

        Assert.Equal("Other Identity Provider", (await TenantOptionsTestHelper.GetSchemeAsync(context, OpenIdConnectDefaults.AuthenticationScheme)).DisplayName);

        options = await GetOpenIdConnectOptionsAsync(context);

        Assert.Equal("https://idp-2.example.com/", options.Authority);
        Assert.StartsWith("https://idp-2.example.com/", options.MetadataAddress);
        Assert.NotSame(configurationManager, options.ConfigurationManager);

        Assert.Equal(shellContextTicks, await TenantOptionsTestHelper.GetShellContextTicksAsync(context));
    }

    [Fact]
    public async Task UpdateServerSettings_EndpointsUnchanged_UpdatesOptionsWithoutReleasingTenant()
    {
        using var context = await TenantOptionsTestHelper.CreateSiteWithFeaturesAsync(OpenIdConstants.Features.Server, OpenIdConstants.Features.Validation);

        // Build the options before updating the settings.
        Assert.Null(await GetServerIssuerAsync(context));
        Assert.Null(await GetValidationIssuerAsync(context));

        var shellContextTicks = await TenantOptionsTestHelper.GetShellContextTicksAsync(context);

        await UpdateServerSettingsAsync(context, settings =>
        {
            settings.Authority = new Uri("https://server.example.com/");
            settings.AccessTokenFormat = OpenIdServerSettings.TokenFormat.JsonWebToken;
        });

        Assert.Equal(new Uri("https://server.example.com/"), await GetServerIssuerAsync(context));
        Assert.Equal(new Uri("https://server.example.com/"), await GetValidationIssuerAsync(context));
        Assert.True(await TenantOptionsTestHelper.GetAsync(context, serviceProvider =>
            serviceProvider.GetRequiredService<IOptionsMonitor<OpenIddictServerDataProtectionOptions>>().CurrentValue.PreferDefaultAccessTokenFormat));

        Assert.Equal(shellContextTicks, await TenantOptionsTestHelper.GetShellContextTicksAsync(context));
    }

    [Fact]
    public async Task UpdateServerSettings_EndpointPathChanged_ReleasesTenant()
    {
        using var context = await TenantOptionsTestHelper.CreateSiteWithFeaturesAsync(OpenIdConstants.Features.Server);

        var shellContextTicks = await TenantOptionsTestHelper.GetShellContextTicksAsync(context);

        await UpdateServerSettingsAsync(context, settings => settings.TokenEndpointPath = PathString.Empty);

        Assert.NotEqual(shellContextTicks, await TenantOptionsTestHelper.GetShellContextTicksAsync(context));
    }

    [Fact]
    public async Task UpdateValidationSettings_AuthorityChanged_UpdatesOptionsWithoutReleasingTenant()
    {
        using var context = await TenantOptionsTestHelper.CreateSiteWithFeaturesAsync(OpenIdConstants.Features.Validation);

        Assert.Null(await TenantOptionsTestHelper.GetSchemeAsync(context, OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme));

        var shellContextTicks = await TenantOptionsTestHelper.GetShellContextTicksAsync(context);

        await UpdateValidationSettingsAsync(context, settings =>
        {
            settings.Tenant = null;
            settings.Authority = new Uri("https://remote.example.com/");
            settings.Audience = "api";
        });

        Assert.NotNull(await TenantOptionsTestHelper.GetSchemeAsync(context, OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme));
        Assert.Equal(new Uri("https://remote.example.com/"), await GetValidationIssuerAsync(context));
        Assert.Equal(shellContextTicks, await TenantOptionsTestHelper.GetShellContextTicksAsync(context));
    }

    [Fact]
    public async Task UpdateServerSettings_OtherTenantValidatesTokens_UpdatesOtherTenantOptionsWithoutReleasingTenants()
    {
        using var serverContext = await TenantOptionsTestHelper.CreateSiteWithFeaturesAsync(OpenIdConstants.Features.Server);
        using var validationContext = await TenantOptionsTestHelper.CreateSiteWithFeaturesAsync(OpenIdConstants.Features.Validation);

        // The server tenant must declare a scope for the tenant that validates its tokens.
        await serverContext.UsingTenantScopeAsync(async scope =>
        {
            var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();
            var descriptor = new OpenIddictScopeDescriptor { Name = "api" };
            descriptor.Resources.Add(OpenIdConstants.Prefixes.Tenant + validationContext.TenantName);

            await scopeManager.CreateAsync(descriptor);
        });

        await UpdateValidationSettingsAsync(validationContext, settings => settings.Tenant = serverContext.TenantName);

        Assert.NotNull(await TenantOptionsTestHelper.GetSchemeAsync(validationContext, OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme));
        Assert.Null(await GetValidationIssuerAsync(validationContext));

        var serverTicks = await TenantOptionsTestHelper.GetShellContextTicksAsync(serverContext);
        var validationTicks = await TenantOptionsTestHelper.GetShellContextTicksAsync(validationContext);

        await UpdateServerSettingsAsync(serverContext, settings => settings.Authority = new Uri("https://server.example.com/"));
        await validationContext.WaitForDeferredTasksAsync(CancellationToken.None);

        Assert.Equal(new Uri("https://server.example.com/"), await GetValidationIssuerAsync(validationContext));
        Assert.Equal(serverTicks, await TenantOptionsTestHelper.GetShellContextTicksAsync(serverContext));
        Assert.Equal(validationTicks, await TenantOptionsTestHelper.GetShellContextTicksAsync(validationContext));
    }

    private static Task<Uri> GetServerIssuerAsync(SiteContext context)
        => TenantOptionsTestHelper.GetAsync(context, serviceProvider =>
            serviceProvider.GetRequiredService<IOptionsMonitor<OpenIddictServerOptions>>().CurrentValue.Issuer);

    private static Task<Uri> GetValidationIssuerAsync(SiteContext context)
        => TenantOptionsTestHelper.GetAsync(context, serviceProvider =>
            serviceProvider.GetRequiredService<IOptionsMonitor<OpenIddictValidationOptions>>().CurrentValue.Issuer);

    private static Task<OpenIdConnectOptions> GetOpenIdConnectOptionsAsync(SiteContext context)
        => TenantOptionsTestHelper.GetAsync(context, serviceProvider =>
            serviceProvider.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>().Get(OpenIdConnectDefaults.AuthenticationScheme));

    private static async Task UpdateServerSettingsAsync(SiteContext context, Action<OpenIdServerSettings> update)
    {
        await context.UsingTenantScopeAsync(async scope =>
        {
            var serverService = scope.ServiceProvider.GetRequiredService<IOpenIdServerService>();

            var settings = await serverService.LoadSettingsAsync();
            update(settings);

            await serverService.UpdateSettingsAsync(settings);
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);
    }

    private static async Task UpdateValidationSettingsAsync(SiteContext context, Action<OpenIdValidationSettings> update)
    {
        await context.UsingTenantScopeAsync(async scope =>
        {
            var validationService = scope.ServiceProvider.GetRequiredService<IOpenIdValidationService>();

            var settings = await validationService.LoadSettingsAsync();
            update(settings);

            await validationService.UpdateSettingsAsync(settings);
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);
    }

    private static Task UpdateClientSettingsAsync(SiteContext context, string authority, string displayName)
        => TenantOptionsTestHelper.UpdateSiteAsync(
            context,
            (site, _) => site.Alter<OpenIdClientSettings>(settings =>
            {
                settings.Authority = new Uri(authority);
                settings.ClientId = "client";
                settings.DisplayName = displayName;
                settings.ResponseType = "code";
                settings.ResponseMode = "form_post";
            }),
            notifier => notifier
                .RequestUpdate<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme)
                .RequestUpdate<AuthenticationOptions>());
}
