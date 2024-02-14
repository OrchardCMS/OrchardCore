using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Settings;

namespace OrchardCore.Email.Services;

public class SmtpOptionsConfiguration : IConfigureOptions<SmtpOptions>
{
    public const string ProtectorName = "SmtpSettingsConfiguration";
    public const string SectionName = "OrchardCore_Email";

    private readonly ISiteService _siteService;
    private readonly IShellConfiguration _shellConfiguration;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger _logger;

    public SmtpOptionsConfiguration(
        ISiteService siteService,
        IShellConfiguration shellConfiguration,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<SmtpOptionsConfiguration> logger)
    {
        _siteService = siteService;
        _shellConfiguration = shellConfiguration;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    public void Configure(SmtpOptions options)
    {
        var fileSettings = _shellConfiguration.GetSection(SectionName);

        if (fileSettings.Exists())
        {
            fileSettings.Bind(options);

            if (HasRequiredSettings(options))
            {
                options.IsEnabled = true;

                return;
            }
            else
            {
                _logger.LogWarning("The SMTP provider settings in the configuration provider are invalid or are incomplete.");
            }
        }

        var settings = _siteService.GetSiteSettingsAsync()
            .GetAwaiter().GetResult()
            .As<SmtpSettings>();

        options.IsEnabled = settings.IsEnabled ?? HasRequiredSettings(settings);
        options.DefaultSender = settings.DefaultSender;
        options.DeliveryMethod = settings.DeliveryMethod;
        options.PickupDirectoryLocation = settings.PickupDirectoryLocation;
        options.Host = settings.Host;
        options.Port = settings.Port;
        options.ProxyHost = settings.ProxyHost;
        options.ProxyPort = settings.ProxyPort;
        options.EncryptionMethod = settings.EncryptionMethod;
        options.AutoSelectEncryption = settings.AutoSelectEncryption;
        options.RequireCredentials = settings.RequireCredentials;
        options.UseDefaultCredentials = settings.UseDefaultCredentials;
        options.UserName = settings.UserName;
        options.Password = settings.Password;
        options.IgnoreInvalidSslCertificate = settings.IgnoreInvalidSslCertificate;

        // Decrypt the password
        if (!string.IsNullOrWhiteSpace(settings.Password))
        {
            try
            {
                var protector = _dataProtectionProvider.CreateProtector(ProtectorName);
                options.Password = protector.Unprotect(settings.Password);
            }
            catch
            {
                _logger.LogError("The Smtp password could not be decrypted. It may have been encrypted using a different key.");
            }
        }
    }

    private static bool HasRequiredSettings(SmtpOptions model)
    {
        if (string.IsNullOrEmpty(model.DefaultSender))
        {
            return false;
        }

        return model.DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory
            || (model.DeliveryMethod == SmtpDeliveryMethod.Network && !string.IsNullOrEmpty(model.Host));
    }

    private static bool HasRequiredSettings(SmtpSettings model)
    {
        if (string.IsNullOrEmpty(model.DefaultSender))
        {
            return false;
        }

        return model.DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory
            || (model.DeliveryMethod == SmtpDeliveryMethod.Network && !string.IsNullOrEmpty(model.Host));
    }
}
