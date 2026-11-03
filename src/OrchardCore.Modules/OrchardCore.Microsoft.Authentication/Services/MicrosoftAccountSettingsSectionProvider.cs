using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Microsoft.Authentication.Services;

internal sealed class MicrosoftAccountSettingsSectionProvider : SiteSettingsSectionProvider<MicrosoftAccountSettings>
{
    private readonly IDataProtector _protector;

    public MicrosoftAccountSettingsSectionProvider(IDataProtectionProvider dataProtectionProvider, ISiteService siteService, IShellReleaseManager shellReleaseManager, IStringLocalizer<ISiteSettingsSectionProvider> localizer)
        : base(siteService, shellReleaseManager, localizer)
    {
        _protector = dataProtectionProvider.CreateProtector(MicrosoftAuthenticationConstants.Features.MicrosoftAccount);
    }

    public override SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "microsoft-account",
        DisplayName = "Microsoft Account authentication",
        FeatureId = MicrosoftAuthenticationConstants.Features.MicrosoftAccount,
        Description = "Tenant Microsoft Account login. Omitted fields retain their values. appSecret is write-only; clearAppSecret removes it. Referencing a secret of the Secrets module is done from the admin page; a new appSecret replaces such a reference.",
        RequiresHttps = true,
        RequiresReload = true,
    };

    protected override Permission ManagePermission => Permissions.ManageMicrosoftAuthentication;

    protected override void Describe(SiteSettingsSectionProperties<MicrosoftAccountSettings> properties)
    {
        properties.String("appId", "The application identifier.", settings => settings.AppId, (settings, value) => settings.AppId = value, required: true);
        properties.ProtectedSecret("appSecret", "The application secret.", _protector, settings => settings.AppSecret, (settings, value) => settings.AppSecret = value, settings => settings.AppSecretSecretName, (settings, value) => settings.AppSecretSecretName = value);
        properties.CallbackPath("callbackPath", "The request path the provider redirects to, or null for the default one.", settings => settings.CallbackPath, (settings, value) => settings.CallbackPath = value);
        properties.Boolean("saveTokens", "Whether to store the access and refresh tokens after a login.", settings => settings.SaveTokens, (settings, value) => settings.SaveTokens = value);
    }
}
