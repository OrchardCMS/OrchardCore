using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Facebook.Login.Settings;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Login.Services;

internal sealed class FacebookLoginSettingsSectionProvider : SiteSettingsSectionProvider<FacebookLoginSettings>
{
    public FacebookLoginSettingsSectionProvider(ISiteService siteService, IShellReleaseManager shellReleaseManager, IStringLocalizer<ISiteSettingsSectionProvider> localizer)
        : base(siteService, shellReleaseManager, localizer)
    {
    }

    public override SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "facebook-login",
        DisplayName = "Meta login",
        FeatureId = FacebookConstants.Features.Login,
        Description = "Tenant Meta login. Omitted fields retain their values.",
        RequiresHttps = true,
        RequiresReload = true,
    };

    protected override Permission ManagePermission => Permissions.ManageFacebookApp;

    protected override void Describe(SiteSettingsSectionProperties<FacebookLoginSettings> properties)
    {
        properties.CallbackPath("callbackPath", "The request path the provider redirects to, or null for the default one.", settings => settings.CallbackPath, (settings, value) => settings.CallbackPath = value);
        properties.Boolean("saveTokens", "Whether to store the access token after a login.", settings => settings.SaveTokens, (settings, value) => settings.SaveTokens = value);
    }
}
