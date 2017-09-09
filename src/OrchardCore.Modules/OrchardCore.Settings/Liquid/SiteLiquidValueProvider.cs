using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Liquid;

namespace OrchardCore.Settings.Liquid
{
    public class SiteLiquidValueProvider : ILiquidValueProvider
    {
        private readonly ISiteService _siteService;

        public SiteLiquidValueProvider(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task PopulateValuesAsync(IDictionary<string, object> values)
        {
            var site = await _siteService.GetSiteSettingsAsync();
            values.Add("Site", site);
        }
    }
}
