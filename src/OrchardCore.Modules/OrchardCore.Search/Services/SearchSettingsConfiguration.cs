using Microsoft.Extensions.Options;
using OrchardCore.Search.Models;
using OrchardCore.Settings;

namespace OrchardCore.Search.Configuration
{
    public class SearchSettingsConfiguration : IConfigureOptions<SearchSettings>
    {
        private readonly ISiteService _siteService;

        public SearchSettingsConfiguration(ISiteService site)
        {
            _siteService = site;
        }

        public void Configure(SearchSettings options)
        {
            var settings = _siteService.GetSettingsAsync<SearchSettings>()
                .GetAwaiter()
                .GetResult();

            options.ProviderName = settings.ProviderName;
        }
    }
}
