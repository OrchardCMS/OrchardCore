using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Settings;

namespace OrchardCore.Email.Smtp.Services;

public sealed class SmtpOptionsConfiguration : IConfigureOptions<SmtpOptions>
{
    public const string ProtectorName = "SmtpSettingsConfiguration";

    private readonly ISiteService _siteService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger _logger;

    public SmtpOptionsConfiguration(
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<SmtpOptionsConfiguration> logger)
    {
        _siteService = siteService;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    public void Configure(SmtpOptions options)
    {
        var settings = _siteService.GetSettingsAsync<SmtpSettings>()
            .GetAwaiter()
            .GetResult();

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

        options.IsEnabled = settings.IsEnabled ?? options.ConfigurationExists();
    }
}
