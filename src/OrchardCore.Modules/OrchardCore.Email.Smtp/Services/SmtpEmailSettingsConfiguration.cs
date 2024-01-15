using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Settings;

namespace OrchardCore.Email.Smtp.Services;

public class SmtpEmailSettingsConfiguration(
    ISiteService site,
    IDataProtectionProvider dataProtectionProvider,
    ILogger<SmtpEmailSettingsConfiguration> logger) : IAsyncConfigureOptions<SmtpEmailSettings>
{
    private readonly ISiteService _site = site;
    private readonly IDataProtectionProvider _dataProtectionProvider = dataProtectionProvider;
    private readonly ILogger _logger = logger;

    public async ValueTask ConfigureAsync(SmtpEmailSettings options)
    {
        var settings = (await _site.GetSiteSettingsAsync()).As<SmtpEmailSettings>();

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
                var protector = _dataProtectionProvider.CreateProtector(nameof(SmtpEmailSettingsConfiguration));
                options.Password = protector.Unprotect(settings.Password);
            }
            catch
            {
                _logger.LogError("The Smtp password could not be decrypted. It may have been encrypted using a different key.");
            }
        }
    }
}
