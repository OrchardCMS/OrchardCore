using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Settings;

namespace Orchard.Hosting.Routing
{
    public class HomePageRoute : Route
    {
        private readonly IRouteBuilder _routeBuilder;

        public HomePageRoute(IRouteBuilder routeBuilder, IInlineConstraintResolver inlineConstraintResolver)
            : base(routeBuilder.DefaultHandler, "", inlineConstraintResolver)
        {
            _routeBuilder = routeBuilder;
        }

        protected override async Task OnRouteMatched(RouteContext context)
        {
            var serviceProvider = _routeBuilder.ServiceProvider;
            var siteService = serviceProvider.GetService<ISiteService>();
            var siteSettings = await siteService.GetSiteSettingsAsync();

            context.RouteData.Values["area"] = siteSettings.HomeRoute["area"];
            context.RouteData.Values["controller"] = siteSettings.HomeRoute["controller"];
            context.RouteData.Values["action"] = siteSettings.HomeRoute["action"];

            await base.OnRouteMatched(context);
        }
    }
}
