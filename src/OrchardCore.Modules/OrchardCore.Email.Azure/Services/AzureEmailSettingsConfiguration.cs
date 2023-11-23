using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Azure;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Email.Services
{
    public class AzureEmailSettingsConfiguration : IConfigureOptions<AzureEmailSettings>
    {
        private readonly IShellConfiguration _shellConfiguration;
        private readonly ISiteService _site;

        public AzureEmailSettingsConfiguration(IShellConfiguration shellConfiguration, ISiteService site)
        {
            _shellConfiguration = shellConfiguration;
            _site = site;
        }

        public void Configure(AzureEmailSettings options)
        {
            var section = _shellConfiguration.GetSection("OrchardCore_Email_Azure");

            var emailSettings = _site.GetSiteSettingsAsync()
                .GetAwaiter().GetResult()
                .As<EmailSettings>();

            options.DefaultSender = section.GetValue(nameof(options.DefaultSender), emailSettings.DefaultSender);
            options.ConnectionString = section.GetValue(nameof(options.ConnectionString), string.Empty);
        }
    }
}
