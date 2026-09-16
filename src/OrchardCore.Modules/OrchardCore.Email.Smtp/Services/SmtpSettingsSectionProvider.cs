using System.Text.Json.Nodes;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Services;
using OrchardCore.Email.Smtp.ViewModels;
using OrchardCore.Entities;
using OrchardCore.Environment.Options;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Email.Smtp.Services;

internal sealed class SmtpSettingsSectionProvider : ISiteSettingsSectionProvider
{
    private readonly ISiteService _siteService;
    private readonly IOptionsMonitor<SmtpOptions> _options;
    private readonly SmtpSettingsEditor _editor;
    private readonly IOptionsUpdateNotifier _notifier;
    private readonly IStringLocalizer S;

    public SmtpSettingsSectionProvider(ISiteService siteService, IOptionsMonitor<SmtpOptions> options,
        IOptionsUpdateNotifier notifier, IDataProtectionProvider protection, IEmailAddressValidator validator,
        IStringLocalizer<SmtpSettingsSectionProvider> localizer)
    {
        _siteService = siteService;
        _options = options;
        _notifier = notifier;
        _editor = new SmtpSettingsEditor(notifier, protection, validator, localizer);
        S = localizer;
    }

    public SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "smtp",
        DisplayName = "SMTP",
        FeatureId = "OrchardCore.Email.Smtp",
        RequiresHttps = true,
        Description = "Tenant SMTP provider settings. Omitted fields retain their values. Password is write-only; clearPassword explicitly removes it. Host default-provider configuration is separate.",
    };

    public Permission ReadPermission => EmailPermissions.ManageEmailSettings;
    public Permission UpdatePermission => EmailPermissions.ManageEmailSettings;

    public JsonObject GetSchema() => new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["isEnabled"] = new JsonObject { ["type"] = "boolean" },
            ["password"] = new JsonObject { ["type"] = "string", ["writeOnly"] = true, ["minLength"] = 1 },
            ["clearPassword"] = new JsonObject { ["type"] = "boolean" },
            ["defaultSender"] = new JsonObject { ["type"] = "string" },
            ["host"] = new JsonObject { ["type"] = "string" },
            ["port"] = new JsonObject { ["type"] = "integer", ["minimum"] = 0, ["maximum"] = 65535 },
            ["autoSelectEncryption"] = new JsonObject { ["type"] = "boolean" },
            ["requireCredentials"] = new JsonObject { ["type"] = "boolean" },
            ["useDefaultCredentials"] = new JsonObject { ["type"] = "boolean" },
            ["userName"] = new JsonObject { ["type"] = "string" },
            ["proxyHost"] = new JsonObject { ["type"] = "string" },
            ["proxyPort"] = new JsonObject { ["type"] = "integer", ["minimum"] = 0, ["maximum"] = 65535 },
            ["ignoreInvalidSslCertificate"] = new JsonObject { ["type"] = "boolean" },
            ["pickupDirectoryLocation"] = new JsonObject { ["type"] = "string" },
            ["deliveryMethod"] = new JsonObject { ["type"] = "string", ["enum"] = new JsonArray("Network", "SpecifiedPickupDirectory") },
            ["encryptionMethod"] = new JsonObject { ["type"] = "string", ["enum"] = new JsonArray("None", "SslTls", "StartTls") },
        },
    };

    public async Task<SiteSettingsSectionResponse> GetAsync()
    {
        var site = await _siteService.GetSiteSettingsAsync();
        return ToResponse(site.GetOrCreate<SmtpSettings>());
    }

    public async Task<SiteSettingsSectionUpdateResult> UpdateAsync(JsonObject values)
    {
        var errors = new Dictionary<string, string[]>();
        if (values is null)
        {
            return new() { Errors = new() { ["body"] = [S["A settings object is required."]] } };
        }
        if (values.Count == 0)
        {
            return new() { Section = await GetAsync() };
        }
        var site = await _siteService.LoadSiteSettingsAsync();
        var current = site.GetOrCreate<SmtpSettings>();
        var model = new SmtpSettingsViewModel
        {
            IsEnabled = current.IsEnabled ?? _options.CurrentValue.ConfigurationExists(),
            DefaultSender = current.DefaultSender,
            Host = current.Host,
            Port = current.Port,
            AutoSelectEncryption = current.AutoSelectEncryption,
            RequireCredentials = current.RequireCredentials,
            UseDefaultCredentials = current.UseDefaultCredentials,
            UserName = current.UserName,
            ProxyHost = current.ProxyHost,
            ProxyPort = current.ProxyPort,
            IgnoreInvalidSslCertificate = current.IgnoreInvalidSslCertificate,
            PickupDirectoryLocation = current.PickupDirectoryLocation,
            DeliveryMethod = current.DeliveryMethod,
            EncryptionMethod = current.EncryptionMethod,
        };
        var clearPassword = false;
        foreach (var entry in values)
        {
            switch (entry.Key)
            {
                case "isEnabled":
                    if (entry.Value is JsonValue nodeIsEnabled && nodeIsEnabled.TryGetValue<bool>(out var isEnabledValue))
                    {
                        model.IsEnabled = isEnabledValue;
                    }
                    else
                    {
                        errors[entry.Key] = [S["Provide a valid value of the documented type; null is not supported."]];
                    }
                    break;
                case "defaultSender":
                    if (entry.Value is JsonValue nodeDefaultSender && nodeDefaultSender.TryGetValue<string>(out var defaultSenderValue))
                    {
                        model.DefaultSender = defaultSenderValue;
                    }
                    else
                    {
                        errors[entry.Key] = [S["Provide a valid value of the documented type; null is not supported."]];
                    }
                    break;
                case "host":
                    if (entry.Value is JsonValue nodeHost && nodeHost.TryGetValue<string>(out var hostValue))
                    {
                        model.Host = hostValue;
                    }
                    else
                    {
                        errors[entry.Key] = [S["Provide a valid value of the documented type; null is not supported."]];
                    }
                    break;
                case "port":
                    if (entry.Value is JsonValue nodePort && nodePort.TryGetValue<int>(out var portValue))
                    {
                        model.Port = portValue;
                    }
                    else
                    {
                        errors[entry.Key] = [S["Provide a valid value of the documented type; null is not supported."]];
                    }
                    break;
                case "autoSelectEncryption":
                    if (entry.Value is JsonValue nodeAutoSelectEncryption && nodeAutoSelectEncryption.TryGetValue<bool>(out var autoSelectEncryptionValue))
                    {
                        model.AutoSelectEncryption = autoSelectEncryptionValue;
                    }
                    else
                    {
                        errors[entry.Key] = [S["Provide a valid value of the documented type; null is not supported."]];
                    }
                    break;
                case "requireCredentials":
                    if (entry.Value is JsonValue nodeRequireCredentials && nodeRequireCredentials.TryGetValue<bool>(out var requireCredentialsValue))
                    {
                        model.RequireCredentials = requireCredentialsValue;
                    }
                    else
                    {
                        errors[entry.Key] = [S["Provide a valid value of the documented type; null is not supported."]];
                    }
                    break;
                case "useDefaultCredentials":
                    if (entry.Value is JsonValue nodeUseDefaultCredentials && nodeUseDefaultCredentials.TryGetValue<bool>(out var useDefaultCredentialsValue))
                    {
                        model.UseDefaultCredentials = useDefaultCredentialsValue;
                    }
                    else
                    {
                        errors[entry.Key] = [S["Provide a valid value of the documented type; null is not supported."]];
                    }
                    break;
                case "userName":
                    if (entry.Value is JsonValue nodeUserName && nodeUserName.TryGetValue<string>(out var userNameValue))
                    {
                        model.UserName = userNameValue;
                    }
                    else
                    {
                        errors[entry.Key] = [S["Provide a valid value of the documented type; null is not supported."]];
                    }
                    break;
                case "proxyHost":
                    if (entry.Value is JsonValue nodeProxyHost && nodeProxyHost.TryGetValue<string>(out var proxyHostValue))
                    {
                        model.ProxyHost = proxyHostValue;
                    }
                    else
                    {
                        errors[entry.Key] = [S["Provide a valid value of the documented type; null is not supported."]];
                    }
                    break;
                case "proxyPort":
                    if (entry.Value is JsonValue nodeProxyPort && nodeProxyPort.TryGetValue<int>(out var proxyPortValue))
                    {
                        model.ProxyPort = proxyPortValue;
                    }
                    else
                    {
                        errors[entry.Key] = [S["Provide a valid value of the documented type; null is not supported."]];
                    }
                    break;
                case "ignoreInvalidSslCertificate":
                    if (entry.Value is JsonValue nodeIgnoreInvalidSslCertificate && nodeIgnoreInvalidSslCertificate.TryGetValue<bool>(out var ignoreInvalidSslCertificateValue))
                    {
                        model.IgnoreInvalidSslCertificate = ignoreInvalidSslCertificateValue;
                    }
                    else
                    {
                        errors[entry.Key] = [S["Provide a valid value of the documented type; null is not supported."]];
                    }
                    break;
                case "pickupDirectoryLocation":
                    if (entry.Value is JsonValue nodePickupDirectoryLocation && nodePickupDirectoryLocation.TryGetValue<string>(out var pickupDirectoryLocationValue))
                    {
                        model.PickupDirectoryLocation = pickupDirectoryLocationValue;
                    }
                    else
                    {
                        errors[entry.Key] = [S["Provide a valid value of the documented type; null is not supported."]];
                    }
                    break;
                case "deliveryMethod":
                    if (entry.Value is JsonValue nodeDeliveryMethod && nodeDeliveryMethod.TryGetValue<string>(out var deliveryMethodValueName) && Enum.GetNames<SmtpDeliveryMethod>().Contains(deliveryMethodValueName, StringComparer.OrdinalIgnoreCase) && Enum.TryParse<SmtpDeliveryMethod>(deliveryMethodValueName, true, out var deliveryMethodValue))
                    {
                        model.DeliveryMethod = deliveryMethodValue;
                    }
                    else
                    {
                        errors[entry.Key] = [S["Provide a valid value of the documented type; null is not supported."]];
                    }
                    break;
                case "encryptionMethod":
                    if (entry.Value is JsonValue nodeEncryptionMethod && nodeEncryptionMethod.TryGetValue<string>(out var encryptionMethodValueName) && Enum.GetNames<SmtpEncryptionMethod>().Contains(encryptionMethodValueName, StringComparer.OrdinalIgnoreCase) && Enum.TryParse<SmtpEncryptionMethod>(encryptionMethodValueName, true, out var encryptionMethodValue))
                    {
                        model.EncryptionMethod = encryptionMethodValue;
                    }
                    else
                    {
                        errors[entry.Key] = [S["Provide a valid value of the documented type; null is not supported."]];
                    }
                    break;
                case "password":
                    if (entry.Value is JsonValue nodePassword && nodePassword.TryGetValue<string>(out var passwordValue) && !string.IsNullOrWhiteSpace(passwordValue))
                    {
                        model.Password = passwordValue;
                    }
                    else
                    {
                        errors[entry.Key] = [S["Provide a valid value of the documented type; null is not supported."]];
                    }
                    break;
                case "clearPassword":
                    if (entry.Value is JsonValue nodeClearPassword && nodeClearPassword.TryGetValue<bool>(out var clearPasswordValue))
                    {
                        clearPassword = clearPasswordValue;
                    }
                    else
                    {
                        errors[entry.Key] = [S["Provide a valid value of the documented type; null is not supported."]];
                    }
                    break;
                default:
                    errors[entry.Key] = [S["This property is not managed by the SMTP section."]];
                    break;
            }
        }
        if (clearPassword && model.Password is not null)
        {
            errors["password"] = [S["Do not set and clear the password in the same request."]];
        }
        if (!model.IsEnabled && values.Any(entry => entry.Key is not "isEnabled" and not "clearPassword"))
        {
            errors["isEnabled"] = [S["Enable the SMTP provider when updating its configuration."]];
        }
        var changed = _editor.Apply(site, current, model,
            (name, message) => errors[char.ToLowerInvariant(name[0]) + name[1..]] = [message],
            () => errors.Count == 0);
        if (errors.Count > 0)
        {
            return new() { Errors = errors };
        }
        if (clearPassword && !string.IsNullOrEmpty(current.Password))
        {
            current.Password = null;
            changed = true;
            _notifier.RequestUpdate<SmtpOptions>();
        }
        if (changed)
        {
            site.Put(current);
            await _siteService.UpdateSiteSettingsAsync(site);
        }
        return new() { Section = ToResponse(current), Changed = changed };
    }

    private SiteSettingsSectionResponse ToResponse(SmtpSettings settings) => new()
    {
        Name = "smtp",
        Source = "tenant",
        RedactedProperties = ["password"],
        Values = new JsonObject
        {
            ["isEnabled"] = settings.IsEnabled ?? _options.CurrentValue.ConfigurationExists(),
            ["hasPassword"] = !string.IsNullOrEmpty(settings.Password),
            ["defaultSender"] = settings.DefaultSender,
            ["host"] = settings.Host,
            ["port"] = settings.Port,
            ["autoSelectEncryption"] = settings.AutoSelectEncryption,
            ["requireCredentials"] = settings.RequireCredentials,
            ["useDefaultCredentials"] = settings.UseDefaultCredentials,
            ["userName"] = settings.UserName,
            ["proxyHost"] = settings.ProxyHost,
            ["proxyPort"] = settings.ProxyPort,
            ["ignoreInvalidSslCertificate"] = settings.IgnoreInvalidSslCertificate,
            ["pickupDirectoryLocation"] = settings.PickupDirectoryLocation,
            ["deliveryMethod"] = settings.DeliveryMethod.ToString(),
            ["encryptionMethod"] = settings.EncryptionMethod.ToString(),
        },
    };
}
