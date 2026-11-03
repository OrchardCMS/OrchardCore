using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.ReverseProxy.Settings;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.ReverseProxy.Services;

internal sealed class ReverseProxySettingsSectionProvider : SiteSettingsSectionProvider<ReverseProxySettings>
{
    private readonly IOptionsSnapshot<ReverseProxySettings> _options;

    public ReverseProxySettingsSectionProvider(IOptionsSnapshot<ReverseProxySettings> options, ISiteService siteService, IShellReleaseManager shellReleaseManager, IStringLocalizer<ISiteSettingsSectionProvider> localizer)
        : base(siteService, shellReleaseManager, localizer)
    {
        _options = options;
    }

    public override SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "reverse-proxy",
        DisplayName = "Reverse proxy",
        FeatureId = "OrchardCore.ReverseProxy",
        Description = "Forwarded headers the tenant trusts. Omitted fields retain their values. Known networks and proxies are not managed here, as in the admin page. Plain HTTP is accepted, since a proxy terminating TLS only looks like HTTPS once its forwarded headers are enabled.",
        RequiresReload = true,
    };

    protected override Permission ManagePermission => Permissions.ManageReverseProxySettings;

    protected override void Describe(SiteSettingsSectionProperties<ReverseProxySettings> properties)
    {
        Flag(properties, "enableXForwardedFor", "Trusts the X-Forwarded-For header.", ForwardedHeaders.XForwardedFor);
        Flag(properties, "enableXForwardedHost", "Trusts the X-Forwarded-Host header.", ForwardedHeaders.XForwardedHost);
        Flag(properties, "enableXForwardedProto", "Trusts the X-Forwarded-Proto header.", ForwardedHeaders.XForwardedProto);
    }

    protected override string GetReadOnlyReason()
        => _options.Value.FromConfiguration ? "These settings are set from configuration, which takes precedence over the tenant settings." : null;

    protected override ReverseProxySettings GetEffectiveSettings(ReverseProxySettings stored)
        => _options.Value.FromConfiguration ? _options.Value : stored;

    private static void Flag(SiteSettingsSectionProperties<ReverseProxySettings> properties, string name, string description, ForwardedHeaders flag)
        => properties.Boolean(
            name,
            description,
            settings => settings.ForwardedHeaders.HasFlag(flag),
            (settings, value) => settings.ForwardedHeaders = value ? settings.ForwardedHeaders | flag : settings.ForwardedHeaders & ~flag);
}
