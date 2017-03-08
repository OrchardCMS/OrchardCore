using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Mvc.HelloWorld
{
    public class Startup : StartupBase
    {
        private readonly IServiceProvider _applicationServices;
        private readonly IConfiguration _configuration;

        public Startup(IServiceProvider applicationServices,
            IConfiguration configuration)
        {
            _applicationServices = applicationServices;
            _configuration = configuration;
        }

        public override void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddRouting();
            serviceCollection.AddMvcModules(_applicationServices);
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            builder.ConfigureModules(apb =>
            {
                apb.UseStaticFilesModules();
            });

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
