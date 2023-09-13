using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Models;
using OrchardCore.Settings;

namespace OrchardCore.Email.Services
{
    public class SmtpSettingsConfiguration : IConfigureOptions<SmtpSettings>
    {
        private readonly ISiteService _site;
        private readonly ISecretService _secretService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger _logger;

        public SmtpSettingsConfiguration(
            ISiteService site,
            ISecretService secretService,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<SmtpSettingsConfiguration> logger)
        {
            _site = site;
            _secretService = secretService;
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

            if (!string.IsNullOrEmpty(settings.PasswordSecret))
            {
                options.PasswordSecret = settings.PasswordSecret;
                options.Password = _secretService.GetSecretAsync<TextSecret>(settings.PasswordSecret).GetAwaiter().GetResult()?.Text;
            }
            else if (!string.IsNullOrWhiteSpace(settings.Password))
            {
                try
                {
                    // Decrypt the password.
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
