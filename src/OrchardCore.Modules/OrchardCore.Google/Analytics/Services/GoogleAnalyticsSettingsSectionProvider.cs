using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Google.Analytics.Settings;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Google.Analytics.Services;

internal sealed class GoogleAnalyticsSettingsSectionProvider : SiteSettingsSectionProvider<GoogleAnalyticsSettings>
{
    public GoogleAnalyticsSettingsSectionProvider(ISiteService siteService, IShellReleaseManager shellReleaseManager, IStringLocalizer<ISiteSettingsSectionProvider> localizer)
        : base(siteService, shellReleaseManager, localizer)
    {
    }

    public override SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "google-analytics",
        DisplayName = "Google Analytics",
        FeatureId = GoogleConstants.Features.GoogleAnalytics,
        Description = "Tenant Google Analytics measurement identifier.",
    };

    protected override Permission ManagePermission => Permissions.ManageGoogleAnalytics;

    protected override void Describe(SiteSettingsSectionProperties<GoogleAnalyticsSettings> properties)
    {
        properties.String("trackingId", "The Google Analytics measurement or tracking identifier.", settings => settings.TrackingID, (settings, value) => settings.TrackingID = value, required: true);
    }
}
