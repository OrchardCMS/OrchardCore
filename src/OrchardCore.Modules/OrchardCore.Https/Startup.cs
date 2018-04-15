using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
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
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IDisplayDriver<ISite>, HttpsSettingsDisplayDriver>();
            services.AddSingleton<IHttpsService, HttpsService>();

            services.AddHttpsRedirection(options => {});
            services.ConfigureOptions<HttpsRedirectionConfiguration>();

            services.AddHsts(options =>
            {
                options.MaxAge = TimeSpan.FromDays(365);
                options.Preload = false;
                options.IncludeSubDomains = true;
            });
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            // Determine if SSL redirects are enabled
            var settings = serviceProvider.GetService<IHttpsService>().GetSettingsAsync().GetAwaiter().GetResult();

            if (settings.RequireHttps)
            {
                var env = serviceProvider.GetService<IHostingEnvironment>();
                if (!env.IsDevelopment())
                {
                    app.UseHsts();
                }

                app.UseHttpsRedirection();
            }
        }
    }
}
