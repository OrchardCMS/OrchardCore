using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Extensions;

namespace Orchard.Mvc
{
    public class Startup : StartupBase
    {
        private readonly IExtensionLibraryService _applicationServices;

        public Startup(IExtensionLibraryService applicationServices)
        {
            _applicationServices = applicationServices;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcModules();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            app.UseMvcModules();
        }
    }
}
