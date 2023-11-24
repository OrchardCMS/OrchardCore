using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Email.Services
{
    public class SmtpEmailSettingsConfiguration : IConfigureOptions<SmtpEmailSettings>
    {
        private readonly ISiteService _site;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger _logger;

        public SmtpEmailSettingsConfiguration(
            ISiteService site,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<SmtpEmailSettingsConfiguration> logger)
        {
            _site = site;
            _dataProtectionProvider = dataProtectionProvider;
            _logger = logger;
        }

        public void Configure(SmtpEmailSettings options)
        {
            var settings = _site.GetSiteSettingsAsync()
                .GetAwaiter().GetResult()
                .As<SmtpEmailSettings>();

            options.DefaultSender = settings.DefaultSender;
            options.DeliveryMethod = settings.DeliveryMethod;
            options.PickupDirectoryLocation = settings.PickupDirectoryLocation;
            options.Host = settings.Host;
            options.Port = settings.Port;
            options.EncryptionMethod = settings.EncryptionMethod;
            options.AutoSelectEncryption = settings.AutoSelectEncryption;
            options.RequireCredentials = settings.RequireCredentials;
            options.UseDefaultCredentials = settings.UseDefaultCredentials;
            options.UserName = settings.UserName;

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
}
