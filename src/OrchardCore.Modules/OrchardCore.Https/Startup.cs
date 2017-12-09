using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Navigation;
using OrchardCore.Https.Drivers;
using OrchardCore.Https.Services;
using OrchardCore.Modules;
using OrchardCore.Settings;

namespace OrchardCore.Https
{
    public class Startup : StartupBase
    {
        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var rewriteOptions = new RewriteOptions();

            // Configure the rewrite options.
            serviceProvider.GetService<IConfigureOptions<RewriteOptions>>().Configure(rewriteOptions);

            app.UseRewriter(rewriteOptions);
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IDisplayDriver<ISite>, HttpsSettingsDisplayDriver>();
            services.AddSingleton<IHttpsService, HttpsService>();
            services.AddSingleton(new RewriteOptions());
            
            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<RewriteOptions>, RewriteOptionsHttpsConfiguration>());
        }
    }
}
