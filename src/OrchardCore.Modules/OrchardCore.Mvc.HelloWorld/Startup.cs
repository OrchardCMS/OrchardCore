using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using OrchardCore.Modules;

namespace OrchardCore.Mvc.HelloWorld
{
    public class Startup : StartupBase
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(_configuration["Sample"]))
            {
                throw new Exception(":(");
            }
            
            routes.MapAreaRoute
            (
                name: "Home",
                areaName: "OrchardCore.Mvc.HelloWorld",
                template: "",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }

    [Feature("OrchardCore.Mvc.HelloWorld.All")]
    public class AllStartup : StartupBase
    {
        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute
            (
                name: "All",
                areaName: "OrchardCore.Mvc.HelloWorld",
                template: "all",
                defaults: new { controller = "All", action = "Index" }
            );
        }
    }

    [Feature("OrchardCore.Mvc.HelloWorld.Contoso")]
    public class ContosoStartup : StartupBase
    {
        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute
            (
                name: "Home.Contoso",
                areaName: "OrchardCore.Mvc.HelloWorld",
                template: "",
                defaults: new { controller = "HomeContoso", action = "Index" }
            );
        }
    }

    [Feature("OrchardCore.Mvc.HelloWorld.Acme")]
    public class AcmeStartup : StartupBase
    {
        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute
            (
                name: "Home.Acme",
                areaName: "OrchardCore.Mvc.HelloWorld",
                template: "",
                defaults: new { controller = "HomeAcme", action = "Index" }
            );
        }
    }
}
