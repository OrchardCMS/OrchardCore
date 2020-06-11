using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Email.Services
{
    public class SmtpSettingsConfiguration : IConfigureOptions<SmtpSettings>
    {
        private readonly ISiteService _site;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger _logger;

        public SmtpSettingsConfiguration(
            ISiteService site,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<SmtpSettingsConfiguration> logger)
        {
            _site = site;
            _dataProtectionProvider = dataProtectionProvider;
            _logger = logger;
        }

        public void Configure(SmtpSettings options)
        {
            var settings = _site.GetSiteSettingsAsync()
                .GetAwaiter().GetResult()
                .As<SmtpSettings>();

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
            if (!String.IsNullOrWhiteSpace(settings.Password))
            {
                try
                {
                    var protector = _dataProtectionProvider.CreateProtector(nameof(SmtpSettingsConfiguration));
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
