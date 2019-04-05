using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using OrchardCore.Settings;

namespace OrchardCore.HomeRoute
{
    public class HomePageRoute
    {
        private RouteValueDictionary _routeValues;
        private readonly ISiteService _siteService;
        private IChangeToken _siteServicechangeToken;

        public HomePageRoute(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task<RouteValueDictionary> GetValuesAsync()
        {
            if (_siteServicechangeToken == null || _siteServicechangeToken.HasChanged)
            {
                var routeValues = (await _siteService.GetSiteSettingsAsync()).HomeRoute;

                lock (this)
                {
                    _routeValues = routeValues;
                    _siteServicechangeToken = _siteService.ChangeToken;
                }
            }

            return _routeValues;
        }
    }
}