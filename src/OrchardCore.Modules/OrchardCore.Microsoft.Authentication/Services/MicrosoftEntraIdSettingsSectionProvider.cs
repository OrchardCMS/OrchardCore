using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Microsoft.Authentication.Services;

internal sealed class MicrosoftEntraIdSettingsSectionProvider : SiteSettingsSectionProvider<AzureADSettings>
{
    public MicrosoftEntraIdSettingsSectionProvider(ISiteService siteService, IShellReleaseManager shellReleaseManager, IStringLocalizer<ISiteSettingsSectionProvider> localizer)
        : base(siteService, shellReleaseManager, localizer)
    {
    }

    public override SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "microsoft-entra-id",
        DisplayName = "Microsoft Entra ID authentication",
        FeatureId = MicrosoftAuthenticationConstants.Features.AAD,
        Description = "Tenant Microsoft Entra ID login. Omitted fields retain their values.",
        RequiresHttps = true,
        RequiresReload = true,
    };

    protected override Permission ManagePermission => Permissions.ManageMicrosoftAuthentication;

    protected override void Describe(SiteSettingsSectionProperties<AzureADSettings> properties)
    {
        properties.String("displayName", "The name shown on the login page.", settings => settings.DisplayName, (settings, value) => settings.DisplayName = value, required: true);
        properties.String("appId", "The application identifier.", settings => settings.AppId, (settings, value) => settings.AppId = value, required: true);
        properties.String("tenantId", "The Microsoft Entra tenant identifier.", settings => settings.TenantId, (settings, value) => settings.TenantId = value, required: true);
        properties.CallbackPath("callbackPath", "The request path the provider redirects to, or null for the default one.", settings => settings.CallbackPath, (settings, value) => settings.CallbackPath = value);
        properties.Boolean("saveTokens", "Whether to store the access and refresh tokens after a login.", settings => settings.SaveTokens, (settings, value) => settings.SaveTokens = value);
    }
}
