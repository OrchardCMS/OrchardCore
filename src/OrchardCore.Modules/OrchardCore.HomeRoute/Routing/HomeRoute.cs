using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using OrchardCore.Settings;

namespace OrchardCore.HomeRoute
{
    public class HomeRoute
    {
        private RouteValueDictionary _routeValues;
        private readonly ISiteService _siteService;
        private IChangeToken _changeToken;

        public HomeRoute(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task<RouteValueDictionary> GetValuesAsync()
        {
            if (_changeToken?.HasChanged ?? true)
            {
                _changeToken = _siteService.ChangeToken;
                _routeValues = (await _siteService.GetSiteSettingsAsync()).HomeRoute;
            }

            return new RouteValueDictionary(_routeValues);
        }
    }
}
