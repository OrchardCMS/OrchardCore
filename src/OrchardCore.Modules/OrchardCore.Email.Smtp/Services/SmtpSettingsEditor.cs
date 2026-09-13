using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using OrchardCore.Email.Services;
using OrchardCore.Email.Smtp.ViewModels;
using OrchardCore.Entities;
using OrchardCore.Environment.Options;
using OrchardCore.Settings;

namespace OrchardCore.Email.Smtp.Services;

internal sealed class SmtpSettingsEditor
{
    private readonly IOptionsUpdateNotifier _optionsUpdateNotifier;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IEmailAddressValidator _emailValidator;
    private readonly IStringLocalizer S;

    public SmtpSettingsEditor(IOptionsUpdateNotifier optionsUpdateNotifier,
        IDataProtectionProvider dataProtectionProvider, IEmailAddressValidator emailValidator,
        IStringLocalizer localizer)
    {
        _optionsUpdateNotifier = optionsUpdateNotifier;
        _dataProtectionProvider = dataProtectionProvider;
        _emailValidator = emailValidator;
        S = localizer;
    }

    public bool Apply(ISite site, SmtpSettings settings, SmtpSettingsViewModel model,
        Action<string, string> addError, Func<bool> isValid)
    {
        if (!Enum.IsDefined(model.DeliveryMethod))
        {
            addError(nameof(model.DeliveryMethod), S["Select a valid delivery method."]);
        }
        if (!Enum.IsDefined(model.EncryptionMethod))
        {
            addError(nameof(model.EncryptionMethod), S["Select a valid encryption method."]);
        }
        if (model.Port is < 0 or > 65535)
        {
            addError(nameof(model.Port), S["The port must be between 0 and 65535."]);
        }
        if (model.ProxyPort is < 0 or > 65535)
        {
            addError(nameof(model.ProxyPort), S["The proxy port must be between 0 and 65535."]);
        }
        model.PickupDirectoryLocation = string.IsNullOrWhiteSpace(model.PickupDirectoryLocation)
            ? SmtpPickupDirectoryResolver.DefaultPickupDirectoryLocation
            : model.PickupDirectoryLocation;

        if (model.IsEnabled)
        {
            if (string.IsNullOrEmpty(model.DefaultSender))
            {
                addError(nameof(model.DefaultSender), S["The Default Sender is a required field."]);
            }
            else if (!_emailValidator.Validate(model.DefaultSender))
            {
                addError(nameof(model.DefaultSender), S["The Default Sender is invalid."]);
            }

            if (model.DeliveryMethod == SmtpDeliveryMethod.Network
                && string.IsNullOrWhiteSpace(model.Host))
            {
                addError(nameof(model.Host), S["The {0} field is required.", "Host name"]);
            }
            else if (model.DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory
                && !SmtpPickupDirectoryResolver.IsValidPickupDirectoryLocation(model.PickupDirectoryLocation))
            {
                addError(nameof(model.PickupDirectoryLocation), S["The pickup directory location is invalid."]);
            }

        }

        if (!isValid())
        {
            return false;
        }

        var emailSettings = site.GetOrCreate<EmailSettings>();

        var hasChanges = model.IsEnabled != settings.IsEnabled;

        if (!model.IsEnabled)
        {
            if (hasChanges && emailSettings.DefaultProviderName == SmtpEmailProvider.TechnicalName)
            {
                emailSettings.DefaultProviderName = null;

                site.Put(emailSettings);
            }

            settings.IsEnabled = false;
        }
        else
        {
            hasChanges |= model.DefaultSender != settings.DefaultSender;
            hasChanges |= model.Host != settings.Host;
            hasChanges |= model.Port != settings.Port;
            hasChanges |= model.AutoSelectEncryption != settings.AutoSelectEncryption;
            hasChanges |= model.RequireCredentials != settings.RequireCredentials;
            hasChanges |= model.UseDefaultCredentials != settings.UseDefaultCredentials;
            hasChanges |= model.EncryptionMethod != settings.EncryptionMethod;
            hasChanges |= model.UserName != settings.UserName;
            hasChanges |= model.ProxyHost != settings.ProxyHost;
            hasChanges |= model.ProxyPort != settings.ProxyPort;
            hasChanges |= model.IgnoreInvalidSslCertificate != settings.IgnoreInvalidSslCertificate;
            hasChanges |= model.DeliveryMethod != settings.DeliveryMethod;
            hasChanges |= model.PickupDirectoryLocation != settings.PickupDirectoryLocation;

            // Store the password when there is a new value.
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                // Encrypt the password.
                var protector = _dataProtectionProvider.CreateProtector(SmtpOptionsConfiguration.ProtectorName);

                var passwordMatches = false;
                if (!string.IsNullOrEmpty(settings.Password))
                {
                    try
                    {
                        passwordMatches = protector.Unprotect(settings.Password) == model.Password;
                    }
                    catch (System.Security.Cryptography.CryptographicException)
                    {
                        // Replace credentials that can no longer be decrypted.
                    }
                }
                if (!passwordMatches)
                {
                    settings.Password = protector.Protect(model.Password);
                    hasChanges = true;
                }
            }

            settings.IsEnabled = true;
            settings.DefaultSender = model.DefaultSender;
            settings.Host = model.Host;
            settings.Port = model.Port;
            settings.AutoSelectEncryption = model.AutoSelectEncryption;
            settings.RequireCredentials = model.RequireCredentials;
            settings.UseDefaultCredentials = model.UseDefaultCredentials;
            settings.EncryptionMethod = model.EncryptionMethod;
            settings.UserName = model.UserName;
            settings.ProxyHost = model.ProxyHost;
            settings.ProxyPort = model.ProxyPort;
            settings.IgnoreInvalidSslCertificate = model.IgnoreInvalidSslCertificate;
            settings.DeliveryMethod = model.DeliveryMethod;
            settings.PickupDirectoryLocation = string.IsNullOrWhiteSpace(model.PickupDirectoryLocation)
                ? SmtpPickupDirectoryResolver.DefaultPickupDirectoryLocation
                : model.PickupDirectoryLocation;
        }

        if (isValid())
        {
            if (settings.IsEnabled == true && string.IsNullOrEmpty(emailSettings.DefaultProviderName))
            {
                // If we are enabling the only provider, set it as the default one.
                emailSettings.DefaultProviderName = SmtpEmailProvider.TechnicalName;
                site.Put(emailSettings);

                hasChanges = true;
            }

            if (hasChanges)
            {
                _optionsUpdateNotifier
                    .RequestUpdate<SmtpOptions>()
                    .RequestUpdate<EmailProviderOptions>()
                    .RequestUpdate<EmailOptions>();
            }
        }

        return hasChanges;
    }
}
