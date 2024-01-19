using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Settings;

namespace OrchardCore.Media.Processing
{
    public class MediaTokenOptionsConfiguration : IAsyncConfigureOptions<MediaTokenOptions>
    {
        private readonly ISiteService _siteService;

        public MediaTokenOptionsConfiguration(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async ValueTask ConfigureAsync(MediaTokenOptions options)
        {
            options.HashKey = (await _siteService.GetSiteSettingsAsync())
                .As<MediaTokenSettings>()
                .HashKey;
        }
    }
}
