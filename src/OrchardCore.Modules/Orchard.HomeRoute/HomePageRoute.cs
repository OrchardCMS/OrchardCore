using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Orchard.Settings;

namespace Orchard.HomeRoute
{
    public class HomePageRoute : Route
    {
        private readonly IRouteBuilder _routeBuilder;
        private readonly ISiteService _siteService;

        public HomePageRoute(ISiteService siteService, IRouteBuilder routeBuilder, IInlineConstraintResolver inlineConstraintResolver)
            : base(routeBuilder.DefaultHandler, "", inlineConstraintResolver)
        {
            _siteService = siteService;
            _routeBuilder = routeBuilder;
        }

        protected override async Task OnRouteMatched(RouteContext context)
        {
            var tokens = await GetHomeRouteValuesAsync();

            if (tokens != null)
            {
                foreach (var entry in tokens)
                {
                    context.RouteData.Values[entry.Key] = entry.Value;
                }
            }         

            await base.OnRouteMatched(context);
        }

        public override VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            object value;

            var tokens = GetHomeRouteValuesAsync().GetAwaiter().GetResult();

            if (tokens == null)
            {
                return null;
            }

            // Return null if it doesn't match the home route values
            foreach (var entry in tokens)
            {
                if (String.Equals(entry.Key, "area", StringComparison.OrdinalIgnoreCase))
                {
                    if (!context.AmbientValues.TryGetValue("area", out value) || !String.Equals(value.ToString(), tokens["area"].ToString(), StringComparison.OrdinalIgnoreCase))
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
            foreach(var key in tokens.Keys)
            {
                context.Values.Remove(key);
            }

            var result = base.GetVirtualPath(context);

            return result;
        }

        private async Task<RouteValueDictionary> GetHomeRouteValuesAsync()
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            return siteSettings.HomeRoute;
        }
    }
}
