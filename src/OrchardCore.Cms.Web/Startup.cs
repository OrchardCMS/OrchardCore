using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Mvc.RazorPages;

namespace OrchardCore.Cms.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOrchardCms()

                // Route defined at the app level.
                .ConfigureTenant((app, routes, sp) =>
                 {
                     routes.MapAreaRoute(
                         name: "AppDemo",
                         areaName: "OrchardCore.Cms.Web",
                         template: "AppDemo",
                         defaults: new { controller = "AppDemo", action = "Index" }
                     );
                 })

                // Razor Page Route defined at the app level.
                .ConfigureTenantServices((collection) =>
                    collection.Configure<RazorPagesOptions>(options =>
                {
                    // Add a custom page route
                    options.Conventions.AddModularPageRoute("/OrchardCore.Cms.Web/Pages/AppHello", "App/Hello");
                }));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseModules();
        }
    }
}