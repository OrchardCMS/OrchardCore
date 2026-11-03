using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using System.Text.Json.Nodes;

namespace OrchardCore.ReCaptcha.Services;

internal sealed class ReCaptchaSettingsSectionProvider : SiteSettingsSectionProvider<ReCaptchaSettings>
{
    private readonly IOptionsUpdateNotifier _optionsUpdateNotifier;

    public ReCaptchaSettingsSectionProvider(IOptionsUpdateNotifier optionsUpdateNotifier, ISiteService siteService, IShellReleaseManager shellReleaseManager, IStringLocalizer<ISiteSettingsSectionProvider> localizer)
        : base(siteService, shellReleaseManager, localizer)
    {
        _optionsUpdateNotifier = optionsUpdateNotifier;
    }

    public override SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "recaptcha",
        DisplayName = "ReCaptcha",
        FeatureId = "OrchardCore.ReCaptcha",
        Description = "Tenant ReCaptcha keys. Omitted fields retain their values. secretKey is write-only; clearSecretKey removes it.",
        RequiresHttps = true,
    };

    protected override Permission ManagePermission => ReCaptchaPermissions.ManageReCaptchaSettings;

    protected override void Describe(SiteSettingsSectionProperties<ReCaptchaSettings> properties)
    {
        properties.String("siteKey", "The public site key.", settings => settings.SiteKey, (settings, value) => settings.SiteKey = value?.Trim());
        properties.Secret("secretKey", "The private key used to verify the answers.", settings => !string.IsNullOrEmpty(settings.SecretKey), (settings, value) => settings.SecretKey = value.Trim(), settings => settings.SecretKey = null);
    }

    protected override void OnChanging(ISite site, ReCaptchaSettings current, ReCaptchaSettings proposed)
        => _optionsUpdateNotifier.RequestUpdate<ReCaptchaSettings>();
}
