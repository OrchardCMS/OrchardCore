using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Orchard.HomeRoute
{
    public class Startup : StartupBase
    {
        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var inlineConstraintResolver = serviceProvider.GetService<IInlineConstraintResolver>();
            var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
            routes.Routes.Add(new HomePageRoute(httpContextAccessor, routes, inlineConstraintResolver));
        }
    }
}
