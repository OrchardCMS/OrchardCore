using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Settings;

namespace OrchardCore.Email.Smtp.Services;

public class SmtpEmailSettingsConfiguration(
    IShellConfiguration shellConfiguration,
    ISiteService site,
    IDataProtectionProvider dataProtectionProvider,
    ILogger<SmtpEmailSettingsConfiguration> logger) : IConfigureOptions<SmtpEmailSettings>
{
    private readonly IShellConfiguration _shellConfiguration = shellConfiguration;
    private readonly ISiteService _site = site;
    private readonly IDataProtectionProvider _dataProtectionProvider = dataProtectionProvider;
    private readonly ILogger _logger = logger;

    public void Configure(SmtpEmailSettings options)
    {
        var emailSettings = _site.GetSiteSettingsAsync()
            .GetAwaiter()
            .GetResult()
            .As<EmailSettings>();

        var smtpEmailSettings = _site.GetSiteSettingsAsync()
            .GetAwaiter()
            .GetResult()
            .As<SmtpEmailSettings>();

        var defaultSender = emailSettings.DefaultSender;

        var section = _shellConfiguration.GetSection("OrchardCore_Email_Smtp");

        options.DefaultSender = section.GetValue(nameof(options.DefaultSender), defaultSender);
        options.DeliveryMethod = smtpEmailSettings.DeliveryMethod;
        options.PickupDirectoryLocation = smtpEmailSettings.PickupDirectoryLocation;
        options.Host =  smtpEmailSettings.Host;
        options.Port = smtpEmailSettings.Port;
        options.ProxyHost = smtpEmailSettings.ProxyHost;
        options.ProxyPort = smtpEmailSettings.ProxyPort;
        options.EncryptionMethod = smtpEmailSettings.EncryptionMethod;
        options.AutoSelectEncryption =  smtpEmailSettings.AutoSelectEncryption;
        options.RequireCredentials = smtpEmailSettings.RequireCredentials;
        options.UseDefaultCredentials = smtpEmailSettings.UseDefaultCredentials;
        options.UserName = smtpEmailSettings.UserName;
        options.Password = smtpEmailSettings.Password;
        options.IgnoreInvalidSslCertificate = smtpEmailSettings.IgnoreInvalidSslCertificate;

        // Decrypt the password
        if (!string.IsNullOrWhiteSpace(options.Password))
        {
            try
            {
                var protector = _dataProtectionProvider.CreateProtector(nameof(SmtpEmailSettingsConfiguration));

                options.Password = protector.Unprotect(options.Password);
            }
            catch
            {
                _logger.LogError("The Smtp password could not be decrypted. It may have been encrypted using a different key.");
            }
        }
    }
}
