using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Google.Authentication.Settings;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Google.Authentication.Services;

internal sealed class GoogleAuthenticationSettingsSectionProvider : SiteSettingsSectionProvider<GoogleAuthenticationSettings>
{
    private readonly IDataProtector _protector;

    public GoogleAuthenticationSettingsSectionProvider(IDataProtectionProvider dataProtectionProvider, ISiteService siteService, IShellReleaseManager shellReleaseManager, IStringLocalizer<ISiteSettingsSectionProvider> localizer)
        : base(siteService, shellReleaseManager, localizer)
    {
        _protector = dataProtectionProvider.CreateProtector(GoogleConstants.Features.GoogleAuthentication);
    }

    public override SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "google-authentication",
        DisplayName = "Google authentication",
        FeatureId = GoogleConstants.Features.GoogleAuthentication,
        Description = "Tenant Google login. Omitted fields retain their values. clientSecret is write-only; clearClientSecret removes it. Referencing a secret of the Secrets module is done from the admin page; a new clientSecret replaces such a reference.",
        RequiresHttps = true,
        RequiresReload = true,
    };

    protected override Permission ManagePermission => Permissions.ManageGoogleAuthentication;

    protected override void Describe(SiteSettingsSectionProperties<GoogleAuthenticationSettings> properties)
    {
        properties.String("clientId", "The OAuth client identifier.", settings => settings.ClientID, (settings, value) => settings.ClientID = value, required: true);
        properties.ProtectedSecret("clientSecret", "The OAuth client secret.", _protector, settings => settings.ClientSecret, (settings, value) => settings.ClientSecret = value, settings => settings.ClientSecretSecretName, (settings, value) => settings.ClientSecretSecretName = value);
        properties.CallbackPath("callbackPath", "The request path the provider redirects to, or null for the default one.", settings => settings.CallbackPath, (settings, value) => settings.CallbackPath = value);
        properties.Boolean("saveTokens", "Whether to store the access and refresh tokens after a login.", settings => settings.SaveTokens, (settings, value) => settings.SaveTokens = value);
    }
}
