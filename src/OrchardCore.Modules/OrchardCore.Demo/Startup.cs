using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.Demo.Commands;
using OrchardCore.Demo.ContentElementDisplays;
using OrchardCore.Demo.Drivers;
using OrchardCore.Demo.Services;
using OrchardCore.Demo.TagHelpers;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Commands;
using OrchardCore.Environment.Navigation;
using OrchardCore.Modules;
using OrchardCore.Mvc;
using OrchardCore.Mvc.RazorPages;
using OrchardCore.Security.Permissions;
using OrchardCore.Users.Models;

namespace OrchardCore.Demo
{
    public class Startup : StartupBase
    {
        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            serviceProvider.AddTagHelpers(typeof(BazTagHelper).Assembly);

            routes.MapAreaRoute(
                name: "Home",
                areaName: "OrchardCore.Demo",
                template: "Home/Index",
                defaults: new { controller = "Home", action = "Index" }
            );

            routes.MapAreaRoute(
                name: "Display",
                areaName: "OrchardCore.Demo",
                template: "Home/Display/{contentItemId}",
                defaults: new { controller = "Home", action = "Display" }
            );

            routes.MapAreaRoute(
                name: "Error",
                areaName: "OrchardCore.Demo",
                template: "Home/IndexError",
                defaults: new { controller = "Home", action = "IndexError" }
            );

            builder.UseMiddleware<NonBlockingMiddleware>();
            builder.UseMiddleware<BlockingMiddleware>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ITestDependency, ClassFoo>();
            services.AddScoped<ICommandHandler, DemoCommands>();
            services.AddSingleton<IBackgroundTask, TestBackgroundTask>();
            services.AddScoped<IShapeTableProvider, DemoShapeProvider>();
            services.AddShapeAttributes<DemoShapeProvider>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IContentDisplayDriver, TestContentElementDisplay>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IPermissionProvider, Permissions>();

            services.AddScoped<IDisplayDriver<User>, UserProfileDisplayDriver>();

            services.Configure<RazorPagesOptions>(options =>
            {
                // The module name (if specified) is already defined as a default folder path
                // options.Conventions.AddModularFolderRoute("/OrchardCore.Demo/Pages", "Orchard Demo");

                // Add a custom folder path
                options.Conventions.AddModularFolderRoute("/OrchardCore.Demo/Pages", "Demo");

                // Add a custom page route
                options.Conventions.AddModularPageRoute("/OrchardCore.Demo/Pages/Hello", "Hello");

                // This declaration would define an home page
                //options.Conventions.AddModularPageRoute("/OrchardCore.Demo/Pages/Hello", "");
            });
        }
    }
}
