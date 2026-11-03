using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Google.TagManager.Settings;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Google.TagManager.Services;

internal sealed class GoogleTagManagerSettingsSectionProvider : SiteSettingsSectionProvider<GoogleTagManagerSettings>
{
    public GoogleTagManagerSettingsSectionProvider(ISiteService siteService, IShellReleaseManager shellReleaseManager, IStringLocalizer<ISiteSettingsSectionProvider> localizer)
        : base(siteService, shellReleaseManager, localizer)
    {
    }

    public override SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "google-tag-manager",
        DisplayName = "Google Tag Manager",
        FeatureId = GoogleConstants.Features.GoogleTagManager,
        Description = "Tenant Google Tag Manager container identifier.",
    };

    protected override Permission ManagePermission => Permissions.ManageGoogleTagManager;

    protected override void Describe(SiteSettingsSectionProperties<GoogleTagManagerSettings> properties)
    {
        properties.String("containerId", "The Google Tag Manager container identifier.", settings => settings.ContainerID, (settings, value) => settings.ContainerID = value, required: true);
    }
}
