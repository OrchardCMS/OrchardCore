using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Settings;

namespace OrchardCore.Email.Services
{
    public class SmtpSettingsConfiguration : IConfigureOptions<SmtpSettings>
    {
        private readonly ISiteService _site;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger<SmtpSettingsConfiguration> _logger;
        private SmtpSettings _smtpSettings;

        public SmtpSettingsConfiguration(
            IShellConfiguration shellConfiguration,
            ISiteService site,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<SmtpSettingsConfiguration> logger)
        {
            _site = site;
            _dataProtectionProvider = dataProtectionProvider;
            _logger = logger;
            _smtpSettings = shellConfiguration.GetSection("OrchardCore.Email").Get<SmtpSettings>();

        }

        public void Configure(SmtpSettings options)
        {
            if (_smtpSettings == null)
            {
                _smtpSettings = _site.GetSiteSettingsAsync()
                    .GetAwaiter().GetResult().As<SmtpSettings>();
            }

            options.DefaultSender = _smtpSettings.DefaultSender;
            options.DeliveryMethod = _smtpSettings.DeliveryMethod;
            options.PickupDirectoryLocation = _smtpSettings.PickupDirectoryLocation;
            options.Host = _smtpSettings.Host;
            options.Port = _smtpSettings.Port;
            options.EncryptionMethod = _smtpSettings.EncryptionMethod;
            options.AutoSelectEncryption = _smtpSettings.AutoSelectEncryption;
            options.RequireCredentials = _smtpSettings.RequireCredentials;
            options.UseDefaultCredentials = _smtpSettings.UseDefaultCredentials;
            options.UserName = _smtpSettings.UserName;

            // Decrypt the password
            if (!String.IsNullOrWhiteSpace(_smtpSettings.Password))
            {
                try
                {
                    var protector = _dataProtectionProvider.CreateProtector(nameof(SmtpSettingsConfiguration));
                    options.Password = protector.Unprotect(_smtpSettings.Password);
                }
                catch
                {
                    _logger.LogError("The Smtp password could not be decrypted. It may have been encrypted using a different key.");
                }
            }
        }
    }
}
