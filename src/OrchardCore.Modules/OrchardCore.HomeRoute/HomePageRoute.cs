using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using OrchardCore.Settings;

namespace OrchardCore.HomeRoute
{
    public class HomePageRoute : Route
    {
        private readonly IRouter _target;

        public HomePageRoute(IRouteBuilder routeBuilder, IInlineConstraintResolver inlineConstraintResolver)
            : base(routeBuilder.DefaultHandler, "", inlineConstraintResolver)
        {
            _target = routeBuilder.DefaultHandler;
        }

        protected override async Task OnRouteMatched(RouteContext context)
        {
            var homeRoute = context.HttpContext.Features.Get<HomeRouteFeature>()?.HomeRoute;

            if (homeRoute != null)
            {
                foreach (var entry in homeRoute)
                {
                    context.RouteData.Values[entry.Key] = entry.Value;
                }
            }

            await base.OnRouteMatched(context);
        }

        public override VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            var homeRoute = context.HttpContext.Features.Get<HomeRouteFeature>()?.HomeRoute;

            if (homeRoute == null || homeRoute.Count == 0)
            {
                return null;
            }

            // Return null if it doesn't match the home route values
            foreach (var entry in homeRoute)
            {
                if (!String.Equals(context.Values[entry.Key]?.ToString(), entry.Value.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
            }

            var path = "/";

            if (context.Values.Count > homeRoute.Count)
            {
                foreach (var entry in context.Values)
                {
                    if (!homeRoute.ContainsKey(entry.Key))
                    {
                        path = QueryHelpers.AddQueryString(path, entry.Key, entry.Value.ToString());
                    }
                }
            }

            return new VirtualPathData(_target, path);
        }
    }
}
