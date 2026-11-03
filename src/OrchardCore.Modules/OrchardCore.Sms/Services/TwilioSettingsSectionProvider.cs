using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.Environment.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Sms.Models;
using System.Text.Json.Nodes;

namespace OrchardCore.Sms.Services;

internal sealed class TwilioSettingsSectionProvider : SiteSettingsSectionProvider<TwilioSettings>
{
    private readonly IOptionsUpdateNotifier _optionsUpdateNotifier;
    private readonly IPhoneFormatValidator _phoneFormatValidator;
    private readonly IDataProtector _protector;

    public TwilioSettingsSectionProvider(IOptionsUpdateNotifier optionsUpdateNotifier, IPhoneFormatValidator phoneFormatValidator, IDataProtectionProvider dataProtectionProvider, ISiteService siteService, IShellReleaseManager shellReleaseManager, IStringLocalizer<ISiteSettingsSectionProvider> localizer)
        : base(siteService, shellReleaseManager, localizer)
    {
        _optionsUpdateNotifier = optionsUpdateNotifier;
        _phoneFormatValidator = phoneFormatValidator;
        _protector = dataProtectionProvider.CreateProtector(TwilioSmsProvider.ProtectorName);
    }

    public override SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "sms-twilio",
        DisplayName = "Twilio SMS provider",
        FeatureId = "OrchardCore.Sms",
        Description = "The Twilio SMS provider of the tenant. Omitted fields retain their values. authToken is write-only; clearAuthToken removes it. Referencing a secret of the Secrets module is done from the admin page; a new authToken replaces such a reference. Enabling the provider requires its phone number, account and token, and makes it the default when there is none; disabling it clears it as the default.",
        RequiresHttps = true,
    };

    protected override Permission ManagePermission => SmsPermissions.ManageSmsSettings;

    protected override void Describe(SiteSettingsSectionProperties<TwilioSettings> properties)
    {
        properties.Boolean("isEnabled", "Whether the provider is enabled.", settings => settings.IsEnabled, (settings, value) => settings.IsEnabled = value);
        properties.String("phoneNumber", "The phone number messages are sent from.", settings => settings.PhoneNumber, (settings, value) => settings.PhoneNumber = value);
        properties.String("accountSid", "The account identifier.", settings => settings.AccountSID, (settings, value) => settings.AccountSID = value);
        properties.ProtectedSecret("authToken", "The auth token.", _protector, settings => settings.AuthToken, (settings, value) => settings.AuthToken = value, settings => settings.AuthTokenSecretName, (settings, value) => settings.AuthTokenSecretName = value);
    }

    protected override Task ValidateAsync(JsonObject values, TwilioSettings proposed, Dictionary<string, string[]> errors)
    {
        if (!proposed.IsEnabled)
        {
            if (values.Any(entry => entry.Key is not "isEnabled" and not "clearAuthToken"))
            {
                errors["isEnabled"] = [S["Enable the Twilio provider when updating its configuration."]];
            }

            return Task.CompletedTask;
        }

        if (string.IsNullOrWhiteSpace(proposed.PhoneNumber))
        {
            errors["phoneNumber"] = [S["The {0} property is required.", "phoneNumber"]];
        }
        else if (!_phoneFormatValidator.IsValid(proposed.PhoneNumber))
        {
            errors["phoneNumber"] = [S["Please provide a valid phone number."]];
        }

        if (string.IsNullOrWhiteSpace(proposed.AccountSID))
        {
            errors["accountSid"] = [S["The {0} property is required.", "accountSid"]];
        }

        if (string.IsNullOrEmpty(proposed.AuthToken) && string.IsNullOrEmpty(proposed.AuthTokenSecretName))
        {
            errors["authToken"] = [S["The {0} property is required.", "authToken"]];
        }

        return Task.CompletedTask;
    }

    protected override void OnChanging(ISite site, TwilioSettings current, TwilioSettings proposed)
    {
        var smsSettings = site.GetOrCreate<SmsSettings>();

        if (!proposed.IsEnabled && current.IsEnabled && smsSettings.DefaultProviderName == TwilioSmsProvider.TechnicalName)
        {
            smsSettings.DefaultProviderName = null;
            site.Put(smsSettings);
        }
        else if (proposed.IsEnabled && string.IsNullOrEmpty(smsSettings.DefaultProviderName))
        {
            // When enabling the only provider, set it as the default one.
            smsSettings.DefaultProviderName = TwilioSmsProvider.TechnicalName;
            site.Put(smsSettings);
        }

        _optionsUpdateNotifier
            .RequestUpdate<TwilioOptions>()
            .RequestUpdate<SmsProviderOptions>();
    }
}
