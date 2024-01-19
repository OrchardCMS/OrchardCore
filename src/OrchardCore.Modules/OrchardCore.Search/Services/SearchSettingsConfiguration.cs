using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Search.Models;
using OrchardCore.Settings;

namespace OrchardCore.Search.Configuration
{
    public class SearchSettingsConfiguration : IAsyncConfigureOptions<SearchSettings>
    {
        private readonly ISiteService _site;

        public SearchSettingsConfiguration(ISiteService site)
        {
            _site = site;
        }

        public async ValueTask ConfigureAsync(SearchSettings options)
        {
            var settings = (await _site.GetSiteSettingsAsync()).As<SearchSettings>();

            options.ProviderName = settings.ProviderName;
        }
    }
}
