using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Settings;

namespace Orchard.HomeRoute
{
    public class Startup : StartupBase
    {
        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var inlineConstraintResolver = serviceProvider.GetService<IInlineConstraintResolver>();
            var siteService = serviceProvider.GetService<ISiteService>();
            routes.Routes.Add(new HomePageRoute(siteService, routes, inlineConstraintResolver));
        }
    }
}
