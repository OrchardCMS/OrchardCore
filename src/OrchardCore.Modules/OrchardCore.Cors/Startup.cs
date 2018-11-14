using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Settings;
using System;
using OrchardCore.Cors.Drivers;
using OrchardCore.Cors.Services;
using CorsService = OrchardCore.Cors.Services.CorsService;

namespace OrchardCore.Cors
{
    public class Startup : StartupBase
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override int Order => -1;

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            app.UseCors();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IDisplayDriver<ISite>, CorsSettingsDisplayDriver>();
            services.AddSingleton<CorsService>();

            services.TryAddEnumerable(ServiceDescriptor
                .Transient<IConfigureOptions<CorsOptions>, CorsOptionsConfiguration>());
        }
    }
}
