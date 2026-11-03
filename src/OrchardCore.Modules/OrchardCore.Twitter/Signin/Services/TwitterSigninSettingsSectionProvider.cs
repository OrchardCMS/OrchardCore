using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Twitter.Signin.Settings;

namespace OrchardCore.Twitter.Signin.Services;

internal sealed class TwitterSigninSettingsSectionProvider : SiteSettingsSectionProvider<TwitterSigninSettings>
{
    public TwitterSigninSettingsSectionProvider(ISiteService siteService, IShellReleaseManager shellReleaseManager, IStringLocalizer<ISiteSettingsSectionProvider> localizer)
        : base(siteService, shellReleaseManager, localizer)
    {
    }

    public override SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "twitter-signin",
        DisplayName = "Sign in with X (Twitter)",
        FeatureId = TwitterConstants.Features.Signin,
        Description = "Tenant X (Twitter) login. Omitted fields retain their values.",
        RequiresHttps = true,
        RequiresReload = true,
    };

    protected override Permission ManagePermission => Permissions.ManageTwitterSignin;

    protected override void Describe(SiteSettingsSectionProperties<TwitterSigninSettings> properties)
    {
        properties.CallbackPath("callbackPath", "The request path the provider redirects to, or null for the default one.", settings => settings.CallbackPath, (settings, value) => settings.CallbackPath = value);
        properties.Boolean("saveTokens", "Whether to store the access token after a login.", settings => settings.SaveTokens, (settings, value) => settings.SaveTokens = value);
    }
}
