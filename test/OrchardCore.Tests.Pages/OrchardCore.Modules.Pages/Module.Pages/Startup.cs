using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace Module.Pages
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RazorPagesOptions>(options =>
            {
                // Add a custom folder route
                options.Conventions.AddAreaFolderRoute("Module.Pages", "/", "");

                // Add a custom page route
                //options.Conventions.AddAreaPageRoute("Module.Pages", "/Foo", "Foo");

                // This declaration would define an home page
                //options.Conventions.AddAreaPageRoute("Module.Pages", "/Foo", "");
            });
        }
    }
}
