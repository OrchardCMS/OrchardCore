using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Navigation;
using OrchardCore.Https.Drivers;
using OrchardCore.Https.Services;
using OrchardCore.Modules;
using OrchardCore.Settings;

namespace OrchardCore.Https
{
    public class Startup : StartupBase
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public Startup(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var rewriteOptions = new RewriteOptions();

            // Configure the rewrite options.
            serviceProvider.GetService<IConfigureOptions<RewriteOptions>>().Configure(rewriteOptions);

            app.UseRewriter(rewriteOptions);

            if (!_hostingEnvironment.IsDevelopment())
            {
                app.UseHsts();
            }
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IDisplayDriver<ISite>, HttpsSettingsDisplayDriver>();
            services.AddSingleton<IHttpsService, HttpsService>();
            services.AddSingleton(new RewriteOptions());
            
            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<RewriteOptions>, RewriteOptionsHttpsConfiguration>());

            services.AddHsts(options =>
            {
                options.Preload = false;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
            });

        }
    }
}
