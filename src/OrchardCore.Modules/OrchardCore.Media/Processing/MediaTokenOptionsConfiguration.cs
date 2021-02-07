using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Media.Processing
{
    public class MediaTokenOptionsConfiguration : IConfigureOptions<MediaTokenOptions>
    {
        private readonly ISiteService _siteService;

        public MediaTokenOptionsConfiguration(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public void Configure(MediaTokenOptions options)
        {
            options.HashKey = _siteService.GetSiteSettingsAsync()
                .GetAwaiter().GetResult()
                .As<MediaTokenSettings>()
                .HashKey;
        }
    }
}
