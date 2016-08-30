using System;
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

        public HomePageRoute(string prefix, ISiteService siteService, IRouteBuilder routeBuilder, IInlineConstraintResolver inlineConstraintResolver)
            : base(routeBuilder.DefaultHandler, prefix ?? "", inlineConstraintResolver)
        {
            _siteService = siteService;
            _routeBuilder = routeBuilder;
        }

        protected override async Task OnRouteMatched(RouteContext context)
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();

            foreach (var entry in siteSettings.HomeRoute)
            {
                context.RouteData.Values[entry.Key] = entry.Value;
            }

            _tokens = siteSettings.HomeRoute;

            await base.OnRouteMatched(context);
        }

        public override VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            object value;

            // Return null if it doesn't match the home route values
            foreach (var entry in _tokens)
            {
                if (String.Equals(entry.Key, "area", StringComparison.OrdinalIgnoreCase))
                {
                    if (!context.AmbientValues.TryGetValue("area", out value) || !String.Equals(value.ToString(), _tokens["area"].ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }
                }
                else
                {
                    if (!context.Values.TryGetValue(entry.Key, out value) || !String.Equals(value.ToString(), entry.Value.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }
                }
            }

            // Remove the values that should not be rendered in the query string
            foreach(var key in _tokens.Keys)
            {
                context.Values.Remove(key);
            }

            var result = base.GetVirtualPath(context);

            return result;
        }
    }
}
