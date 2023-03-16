using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Search.Models;
using OrchardCore.Settings;

namespace OrchardCore.Search.Configuration
{
    public class SearchSettingsConfiguration : IConfigureOptions<SearchSettings>
    {
        private readonly ISiteService _site;

        public SearchSettingsConfiguration(ISiteService site)
        {
            _site = site;
        }

        public void Configure(SearchSettings options)
        {
            var settings = _site.GetSiteSettingsAsync()
                .GetAwaiter().GetResult()
                .As<SearchSettings>();

            options.ProviderName = settings.ProviderName;
        }
    }
}
