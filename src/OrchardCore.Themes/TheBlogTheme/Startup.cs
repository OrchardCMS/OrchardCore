using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace TheBlogTheme
{
    public class Startup : StartupBase
    {
        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RazorPagesOptions>(options =>
            {
                // Add a custom folder route
                options.Conventions.AddAreaFolderRoute("TheBlogTheme", "/", "Theme");

                // Add a custom page route
                options.Conventions.AddAreaPageRoute("TheBlogTheme", "/Bar1", "Bar1");

                // This declaration would define an home page
                //options.Conventions.AddAreaPageRoute("TheBlogTheme", "/Bar1", "");
            });
        }
    }
}
