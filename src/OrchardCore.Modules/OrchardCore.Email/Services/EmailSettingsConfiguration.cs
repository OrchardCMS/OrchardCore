using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Email.Services
{
    public class EmailSettingsConfiguration : IConfigureOptions<EmailSettings>
    {
        private readonly ISiteService _site;

        public EmailSettingsConfiguration(ISiteService site)
        {
            _site = site;
        }

        public void Configure(EmailSettings options)
        {
            var settings = _site.GetSiteSettingsAsync()
                .GetAwaiter().GetResult()
                .As<EmailSettings>();

            options.DefaultSender = settings.DefaultSender;
        }
    }
}
