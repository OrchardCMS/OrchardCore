using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell;
using OrchardCore.ReverseProxy.Settings;
using OrchardCore.ReverseProxy.ViewModels;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Settings.Drivers;

namespace OrchardCore.ReverseProxy.Drivers;

public sealed class ReverseProxySettingsDisplayDriver : ConfigurableSiteSettingsDisplayDriver<ReverseProxySettings, ReverseProxySettingsViewModel>
{
    public const string GroupId = "ReverseProxy";

    public ReverseProxySettingsDisplayDriver(
        IConfigurableSettingsService<ReverseProxySettings> settingsService,
        IShellReleaseManager shellReleaseManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService)
        : base(settingsService, shellReleaseManager, httpContextAccessor, authorizationService)
    {
    }

    protected override string SettingsGroupId => GroupId;

    protected override string EditShapeType => "ReverseProxySettings_Edit";

    protected override Permission RequiredPermission => Permissions.ManageReverseProxySettings;

    protected override void PopulateViewModel(ReverseProxySettingsViewModel model, ReverseProxySettings databaseSettings, ReverseProxySettings effectiveSettings)
    {
        // Use database settings for form fields (what user will edit)
        model.EnableXForwardedFor = databaseSettings.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedFor);
        model.EnableXForwardedHost = databaseSettings.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedHost);
        model.EnableXForwardedProto = databaseSettings.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedProto);
        model.KnownNetworks = string.Join(System.Environment.NewLine, databaseSettings.KnownNetworks ?? []);
        model.KnownProxies = string.Join(System.Environment.NewLine, databaseSettings.KnownProxies ?? []);
    }

    protected override void UpdateSettings(ReverseProxySettings settings, ReverseProxySettingsViewModel model, SettingsConfigurationMetadata metadata)
    {
        // Only update properties that can be configured via UI
        if (ShouldUpdateProperty(nameof(ReverseProxySettings.ForwardedHeaders), metadata))
        {
            settings.ForwardedHeaders = ForwardedHeaders.None;

            if (model.EnableXForwardedFor)
            {
                settings.ForwardedHeaders |= ForwardedHeaders.XForwardedFor;
            }

            if (model.EnableXForwardedHost)
            {
                settings.ForwardedHeaders |= ForwardedHeaders.XForwardedHost;
            }

            if (model.EnableXForwardedProto)
            {
                settings.ForwardedHeaders |= ForwardedHeaders.XForwardedProto;
            }
        }

        if (ShouldUpdateProperty(nameof(ReverseProxySettings.KnownNetworks), metadata))
        {
            settings.KnownNetworks = model.KnownNetworks?
                .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];
        }

        if (ShouldUpdateProperty(nameof(ReverseProxySettings.KnownProxies), metadata))
        {
            settings.KnownProxies = model.KnownProxies?
                .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];
        }
    }
}
