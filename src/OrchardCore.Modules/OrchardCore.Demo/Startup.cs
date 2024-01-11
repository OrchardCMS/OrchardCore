using System;
using Fluid;
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
using OrchardCore.Demo.Controllers;
using OrchardCore.Demo.Drivers;
using OrchardCore.Demo.Models;
using OrchardCore.Demo.Services;
using OrchardCore.Demo.TagHelpers;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Commands;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;

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
                name: "Demo.Home.Index",
                areaName: "OrchardCore.Demo",
                pattern: "Home/Index",
                defaults: new { controller = "Home", action = "Index" }
            );

            routes.MapAreaControllerRoute(
                name: "Demo.Home.Display",
                areaName: "OrchardCore.Demo",
                pattern: "Home/Display/{contentItemId}",
                defaults: new { controller = "Home", action = "Display" }
            );

            routes.MapAreaControllerRoute(
                name: "Demo.Home.Error",
                areaName: "OrchardCore.Demo",
                pattern: "Home/IndexError",
                defaults: new { controller = "Home", action = "IndexError" }
            );

            var demoAdminControllerName = typeof(AdminController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "Demo.Admin",
                areaName: "OrchardCore.Demo",
                pattern: _adminOptions.AdminUrlPrefix + "/Demo/Admin",
                defaults: new { controller = demoAdminControllerName, action = nameof(AdminController.Index) }
            );

            var demoContentControllerName = typeof(ContentController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "Demo.Content.Edit",
                areaName: "OrchardCore.Demo",
                pattern: _adminOptions.AdminUrlPrefix + "/Demo/Content/Edit",
                defaults: new { controller = demoContentControllerName, action = nameof(ContentController.Edit) }
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
            services.AddScoped<IContentDisplayDriver, TestContentElementDisplayDriver>();
            services.AddDataMigration<Migrations>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddContentPart<TestContentPartA>();
            services.AddScoped<IUserClaimsProvider, UserProfileClaimsProvider>();

            services.AddScoped<IDisplayDriver<User>, UserProfileDisplayDriver>();

            services.Configure<RazorPagesOptions>(options =>
            {
                // Add a custom page folder route (only applied to non admin pages)
                options.Conventions.AddAreaFolderRoute("OrchardCore.Demo", "/", "Demo");

                // Add a custom admin page folder route (only applied to admin pages) using the current admin prefix
                options.Conventions.AddAdminAreaFolderRoute("OrchardCore.Demo", "/Admin", _adminOptions.AdminUrlPrefix + "/Demo");

                // Add a custom admin page folder route without using the current admin prefix
                options.Conventions.AddAdminAreaFolderRoute("OrchardCore.Demo", "/Foo/Admin", "Manage/Foo");

                // Add a custom admin page route using the current admin prefix
                options.Conventions.AddAreaPageRoute("OrchardCore.Demo", "/OutsideAdmin", _adminOptions.AdminUrlPrefix + "/Outside");

                // Add a custom page route
                options.Conventions.AddAreaPageRoute("OrchardCore.Demo", "/Hello", "Hello");

                // This declaration would define an home page
                //options.Conventions.AddAreaPageRoute("OrchardCore.Demo", "/Hello", "");
            });

            services.AddTagHelpers(typeof(BazTagHelper).Assembly);

            services.Configure<TemplateOptions>(o =>
            {
                o.MemberAccessStrategy.Register<OrchardCore.Demo.ViewModels.TodoViewModel>();
            });
        }
    }
}
