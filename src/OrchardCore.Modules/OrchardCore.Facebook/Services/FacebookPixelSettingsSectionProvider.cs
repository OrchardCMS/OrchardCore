using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Facebook.Settings;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Services;

internal sealed class FacebookPixelSettingsSectionProvider : SiteSettingsSectionProvider<FacebookPixelSettings>
{
    private readonly IDataProtector _protector;

    public FacebookPixelSettingsSectionProvider(IDataProtectionProvider dataProtectionProvider, ISiteService siteService, IShellReleaseManager shellReleaseManager, IStringLocalizer<ISiteSettingsSectionProvider> localizer)
        : base(siteService, shellReleaseManager, localizer)
    {
        _protector = dataProtectionProvider.CreateProtector(FacebookConstants.ConversionsApiProtectorName);
    }

    public override SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "facebook-pixel",
        DisplayName = "Meta Pixel",
        FeatureId = FacebookConstants.Features.Pixel,
        Description = "Tenant Meta Pixel and Conversions API. Omitted fields retain their values. conversionsApiAccessToken is write-only; clearConversionsApiAccessToken removes it.",
        RequiresHttps = true,
    };

    protected override Permission ManagePermission => Permissions.ManageFacebookApp;

    protected override void Describe(SiteSettingsSectionProperties<FacebookPixelSettings> properties)
    {
        properties.String("pixelId", "The identifier of the pixel.", settings => settings.PixelId, (settings, value) => settings.PixelId = value?.Trim());
        properties.ProtectedSecret("conversionsApiAccessToken", "The Conversions API access token.", _protector, settings => settings.ConversionsApiAccessToken, (settings, value) => settings.ConversionsApiAccessToken = value);
        properties.String("conversionsApiTestEventCode", "The test event code sent with the Conversions API requests.", settings => settings.ConversionsApiTestEventCode, (settings, value) => settings.ConversionsApiTestEventCode = value?.Trim());
    }
}
