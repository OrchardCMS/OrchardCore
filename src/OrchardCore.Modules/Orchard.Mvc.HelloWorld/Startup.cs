using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace Orchard.Mvc.HelloWorld
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
                areaName: "Orchard.Mvc.HelloWorld",
                template: "",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }
}
