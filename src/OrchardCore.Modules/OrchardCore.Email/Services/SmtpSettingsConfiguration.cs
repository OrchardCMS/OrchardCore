using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;

namespace OrchardCore.Email.Services
{
    public class SmtpSettingsConfiguration : IConfigureOptions<SmtpSettings>
    {
        private readonly ISiteService _site;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ShellSettings _shellSettings;

        public SmtpSettingsConfiguration(ISiteService site,
            IDataProtectionProvider dataProtectionProvider,
            ShellSettings shellSettings)
        {
            _site = site;
            _dataProtectionProvider = dataProtectionProvider;
            _shellSettings = shellSettings;
        }

        public void Configure(SmtpSettings options)
        {
            var settings = _site.GetSiteSettingsAsync()
                .GetAwaiter().GetResult()
                .As<SmtpSettings>();

            options.DefaultSender = settings.DefaultSender;
            options.Host = settings.Host;
            options.Port = settings.Port;
            options.EnableSsl = settings.EnableSsl;
            options.RequireCredentials = settings.RequireCredentials;
            options.UseDefaultCredentials = settings.UseDefaultCredentials;
            options.UserName = settings.UserName;

            // Decrypt the password
            if (!String.IsNullOrWhiteSpace(settings.Password))
            {
                var protector = _dataProtectionProvider.CreateProtector(nameof(SmtpSettingsConfiguration), _shellSettings.Name);
                options.Password = protector.Unprotect(settings.Password);
            }
        }
    }
}
