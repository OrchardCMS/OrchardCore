using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using OrchardCore.Settings;

namespace OrchardCore.HomeRoute
{
    public class HomePageRoute : Route
    {
        private RouteValueDictionary _homeRoute;
        private IChangeToken _siteServicechangeToken;

        public HomePageRoute(IRouteBuilder routeBuilder, IInlineConstraintResolver inlineConstraintResolver)
            : base(routeBuilder.DefaultHandler, "", inlineConstraintResolver)
        {
        }

        protected override async Task OnRouteMatched(RouteContext context)
        {
            var tokens = GetHomeRouteValues(context.HttpContext);

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
            var tokens = GetHomeRouteValues(context.HttpContext);

            if (tokens == null || tokens.Count == 0)
            {
                return null;
            }

            // Return null if it doesn't match the home route values
            foreach (var entry in tokens)
            {
                object value;
                if (string.Equals(entry.Key, "area", StringComparison.OrdinalIgnoreCase))
                {
                    if (!context.Values.TryGetValue("area", out value) || !String.Equals(value.ToString(), tokens["area"].ToString(), StringComparison.OrdinalIgnoreCase))
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
            var clonedContext = new VirtualPathContext(context.HttpContext, context.AmbientValues, new RouteValueDictionary(context.Values), context.RouteName);
            foreach (var key in tokens.Keys)
            {
                clonedContext.Values.Remove(key);
            }

            var result = base.GetVirtualPath(clonedContext);

            return result;
        }

        private RouteValueDictionary GetHomeRouteValues(HttpContext httpContext)
        {
            if (_siteServicechangeToken == null || _siteServicechangeToken.HasChanged)
            {
                var siteService = httpContext.RequestServices.GetRequiredService<ISiteService>();
                _homeRoute = siteService.GetSiteSettingsAsync().GetAwaiter().GetResult().HomeRoute;
                _siteServicechangeToken = siteService.ChangeToken;
            }

            return _homeRoute;
        }
    }
}