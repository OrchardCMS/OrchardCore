using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.Demo.Commands;
using OrchardCore.Demo.ContentElementDisplays;
using OrchardCore.Demo.Drivers;
using OrchardCore.Demo.Models;
using OrchardCore.Demo.Services;
using OrchardCore.Demo.TagHelpers;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Commands;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Users.Models;

namespace OrchardCore.Demo
{
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "Home",
                areaName: "OrchardCore.Demo",
                pattern: "Home/Index",
                defaults: new { controller = "Home", action = "Index" }
            );

            routes.MapAreaControllerRoute(
                name: "Display",
                areaName: "OrchardCore.Demo",
                pattern: "Home/Display/{contentItemId}",
                defaults: new { controller = "Home", action = "Display" }
            );

            routes.MapAreaControllerRoute(
                name: "Error",
                areaName: "OrchardCore.Demo",
                pattern: "Home/IndexError",
                defaults: new { controller = "Home", action = "IndexError" }
            );

            routes.MapAreaControllerRoute(
                name: "AdminDemo",
                areaName: "OrchardCore.Demo",
                pattern: _adminOptions.AdminUrlPrefix + "/Demo/Index",
                defaults: new { controller = "Admin", action = "Index" }
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
            services.AddContentPart<TestContentPartA>();

            services.AddScoped<IDisplayDriver<User>, UserProfileDisplayDriver>();

            services.Configure<RazorPagesOptions>(options =>
            {
                // Add a custom folder route
                options.Conventions.AddAreaFolderRoute("OrchardCore.Demo", "/", "Demo");

                // Add a custom page route
                options.Conventions.AddAreaPageRoute("OrchardCore.Demo", "/Hello", "Hello");

                // This declaration would define an home page
                //options.Conventions.AddAreaPageRoute("OrchardCore.Demo", "/Hello", "");
            });

            services.AddTagHelpers(typeof(BazTagHelper).Assembly);
        }
    }
}
