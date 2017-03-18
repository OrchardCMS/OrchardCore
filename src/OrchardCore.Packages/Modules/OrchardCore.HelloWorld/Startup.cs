using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.HelloWorld
{
    public class Startup : StartupBase
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; set; }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute
            (
                name: "HelloWorld",
                areaName: "OrchardCore.HelloWorld",
                template: "HelloWorld",
                defaults: new { controller = "Home", action = "Index" }
            );
        }

        public override void ConfigureServices(IServiceCollection serviceCollection)
        {

        }
    }
}