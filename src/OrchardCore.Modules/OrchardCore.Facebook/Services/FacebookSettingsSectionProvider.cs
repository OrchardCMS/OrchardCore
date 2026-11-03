using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Facebook.Settings;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Services;

internal sealed class FacebookSettingsSectionProvider : SiteSettingsSectionProvider<FacebookSettings>
{
    private readonly IDataProtector _protector;

    public FacebookSettingsSectionProvider(IDataProtectionProvider dataProtectionProvider, ISiteService siteService, IShellReleaseManager shellReleaseManager, IStringLocalizer<ISiteSettingsSectionProvider> localizer)
        : base(siteService, shellReleaseManager, localizer)
    {
        _protector = dataProtectionProvider.CreateProtector(FacebookConstants.Features.Core);
    }

    public override SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "facebook",
        DisplayName = "Meta application",
        FeatureId = FacebookConstants.Features.Core,
        Description = "Tenant Meta application. Omitted fields retain their values. appSecret is write-only; clearAppSecret removes it. Referencing a secret of the Secrets module is done from the admin page; a new appSecret replaces such a reference.",
        RequiresHttps = true,
        RequiresReload = true,
    };

    protected override Permission ManagePermission => Permissions.ManageFacebookApp;

    protected override void Describe(SiteSettingsSectionProperties<FacebookSettings> properties)
    {
        properties.String("appId", "The application identifier.", settings => settings.AppId, (settings, value) => settings.AppId = value, required: true);
        properties.ProtectedSecret("appSecret", "The application secret.", _protector, settings => settings.AppSecret, (settings, value) => settings.AppSecret = value, settings => settings.AppSecretSecretName, (settings, value) => settings.AppSecretSecretName = value);
        properties.String("sdkJs", "The file name of the SDK script.", settings => settings.SdkJs, (settings, value) => settings.SdkJs = value, required: true);
        properties.Boolean("fbInit", "Whether to call FB.init when the SDK loads.", settings => settings.FBInit, (settings, value) => settings.FBInit = value);
        properties.String("fbInitParams", "The parameters of FB.init. An empty value keeps the current ones.", settings => settings.FBInitParams, (settings, value) => settings.FBInitParams = string.IsNullOrWhiteSpace(value) ? settings.FBInitParams : value);
        properties.String("version", "The Graph API version, such as v3.2.", settings => settings.Version, (settings, value) => settings.Version = value, pattern: @"(v)\d+\.\d+");
    }
}
