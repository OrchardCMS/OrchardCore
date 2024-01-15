using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Settings;

namespace OrchardCore.Email.Services
{
    public class EmailSettingsConfiguration(ISiteService site) : IAsyncConfigureOptions<EmailSettings>
    {
        private readonly ISiteService _site = site;

        public async ValueTask ConfigureAsync(EmailSettings options)
        {
            var settings = (await _site.GetSiteSettingsAsync()).As<EmailSettings>();

            options.DefaultSender = settings.DefaultSender;
        }
    }
}
