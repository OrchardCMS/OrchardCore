using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using System.Text.Json.Nodes;

namespace OrchardCore.Sms.Services;

internal sealed class SmsSettingsSectionProvider : SiteSettingsSectionProvider<SmsSettings>
{
    private readonly IOptionsMonitor<SmsProviderOptions> _smsProviderOptions;

    public SmsSettingsSectionProvider(IOptionsMonitor<SmsProviderOptions> smsProviderOptions, ISiteService siteService, IShellReleaseManager shellReleaseManager, IStringLocalizer<ISiteSettingsSectionProvider> localizer)
        : base(siteService, shellReleaseManager, localizer)
    {
        _smsProviderOptions = smsProviderOptions;
    }

    public override SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "sms",
        DisplayName = "SMS",
        FeatureId = "OrchardCore.Sms",
        Description = "The default SMS provider of the tenant, one of the enabled providers. Null disables sending.",
    };

    protected override Permission ManagePermission => SmsPermissions.ManageSmsSettings;

    protected override void Describe(SiteSettingsSectionProperties<SmsSettings> properties)
    {
        properties.String("defaultProvider", "The name of an enabled SMS provider, or null.", settings => settings.DefaultProviderName, (settings, value) => settings.DefaultProviderName = value);
    }

    protected override Task ValidateAsync(JsonObject values, SmsSettings proposed, Dictionary<string, string[]> errors)
    {
        // The admin page only offers the enabled providers.
        if (proposed.DefaultProviderName is { } name
            && !(_smsProviderOptions.CurrentValue.Providers.TryGetValue(name, out var provider) && provider.IsEnabled))
        {
            errors["defaultProvider"] = [S["The SMS provider '{0}' does not exist or is not enabled.", name]];
        }

        return Task.CompletedTask;
    }
}
