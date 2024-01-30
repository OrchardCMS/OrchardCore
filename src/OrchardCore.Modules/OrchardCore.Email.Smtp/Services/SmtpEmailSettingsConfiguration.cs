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
        var section = _shellConfiguration.GetSection("OrchardCore_Email_Smtp");

        var emailSettings = _site.GetSiteSettingsAsync().GetAwaiter().GetResult().As<SmtpEmailSettings>();

        options.DefaultSender = section.GetValue(nameof(options.DefaultSender), emailSettings.DefaultSender);
        options.DeliveryMethod = section.GetValue(nameof(options.DeliveryMethod), emailSettings.DeliveryMethod);
        options.PickupDirectoryLocation = section.GetValue(nameof(options.PickupDirectoryLocation), emailSettings.PickupDirectoryLocation);
        options.Host = section.GetValue(nameof(options.Host), emailSettings.Host);
        options.Port = section.GetValue(nameof(options.Port), emailSettings.Port);
        options.ProxyHost = section.GetValue(nameof(options.ProxyHost), emailSettings.ProxyHost);
        options.ProxyPort = section.GetValue(nameof(options.ProxyPort), emailSettings.ProxyPort);
        options.EncryptionMethod = section.GetValue(nameof(options.EncryptionMethod), emailSettings.EncryptionMethod);
        options.AutoSelectEncryption = section.GetValue(nameof(options.AutoSelectEncryption), emailSettings.AutoSelectEncryption);
        options.RequireCredentials = section.GetValue(nameof(options.RequireCredentials), emailSettings.RequireCredentials);
        options.UseDefaultCredentials = section.GetValue(nameof(options.UseDefaultCredentials), emailSettings.UseDefaultCredentials);
        options.UserName = section.GetValue(nameof(options.UserName), emailSettings.UserName);
        options.Password = section.GetValue(nameof(options.Password), emailSettings.Password);
        options.IgnoreInvalidSslCertificate = section.GetValue(nameof(options.IgnoreInvalidSslCertificate), emailSettings.IgnoreInvalidSslCertificate);

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
