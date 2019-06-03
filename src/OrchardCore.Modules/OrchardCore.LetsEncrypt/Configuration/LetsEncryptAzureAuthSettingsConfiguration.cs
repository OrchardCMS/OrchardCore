using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.LetsEncrypt.Settings;
using OrchardCore.Settings;

namespace OrchardCore.LetsEncrypt.Configuration
{
    public class LetsEncryptAzureAuthSettingsConfiguration : IConfigureOptions<LetsEncryptAzureAuthSettings>
    {
        private readonly ISiteService _site;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger<LetsEncryptAzureAuthSettingsConfiguration> _logger;

        public LetsEncryptAzureAuthSettingsConfiguration(
            ISiteService site,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<LetsEncryptAzureAuthSettingsConfiguration> logger)
        {
            _site = site;
            _dataProtectionProvider = dataProtectionProvider;
            _logger = logger;
        }

        public void Configure(LetsEncryptAzureAuthSettings options)
        {
            var settings = _site.GetSiteSettingsAsync()
                .GetAwaiter().GetResult()
                .As<LetsEncryptAzureAuthSettings>();

            options.Tenant = settings.Tenant;
            options.SubscriptionId = settings.SubscriptionId;
            options.ClientId = settings.ClientId;
            options.ResourceGroupName = settings.ResourceGroupName;
            options.ServicePlanResourceGroupName = settings.ServicePlanResourceGroupName;
            options.UseIPBasedSSL = settings.UseIPBasedSSL;
            options.WebAppName = settings.WebAppName;
            options.SiteSlotName = settings.SiteSlotName;

            // Decrypt the client secret
            if (!String.IsNullOrWhiteSpace(settings.ClientSecret))
            {
                try
                {
                    var protector = _dataProtectionProvider.CreateProtector(nameof(LetsEncryptAzureAuthSettingsConfiguration));
                    options.ClientSecret = protector.Unprotect(settings.ClientSecret);
                }
                catch
                {
                    _logger.LogError("The client secret could not be decrypted. It may have been encrypted using a different key.");
                }
            }
        }
    }
}
