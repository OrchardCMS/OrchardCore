using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Settings;

namespace Orchard.Hosting.Routing
{
    public class HomePageRoute : Route
    {
        private readonly IRouteBuilder _routeBuilder;

        private RouteValueDictionary _tokens;
        private readonly ISiteService _siteService;

        public HomePageRoute(string prefix, IRouteBuilder routeBuilder, IInlineConstraintResolver inlineConstraintResolver)
            : base(routeBuilder.DefaultHandler, prefix ?? "", inlineConstraintResolver)
        {
            _siteService = routeBuilder.ServiceProvider.GetRequiredService<ISiteService>();
            _routeBuilder = routeBuilder;
        }

        protected override async Task OnRouteMatched(RouteContext context)
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();

            foreach (var key in siteSettings.HomeRoute.Keys)
            {
                context.RouteData.Values[key] = siteSettings.HomeRoute[key];
            }

            _tokens = siteSettings.HomeRoute;

            await base.OnRouteMatched(context);
        }

        public override VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            var result = base.GetVirtualPath(context);

            // Return null if it doesn't match the home route values
            foreach (var key in _tokens.Keys)
            {
                object value;
                if (!context.Values.TryGetValue(key, out value) || value.ToString() != _tokens[key].ToString())
                {
                    return null;
                }
            }

            return result;
        }
    }
}
