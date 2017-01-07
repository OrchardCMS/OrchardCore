using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.BackgroundTasks;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.Data.Migration;
using Orchard.Demo.Commands;
using Orchard.Demo.ContentElementDisplays;
using Orchard.Demo.Services;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Commands;
using Orchard.Environment.Navigation;

namespace Orchard.Demo
{
    public class Startup : StartupBase
    {
        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {

            routes.MapAreaRoute(
                name: "Home",
                areaName: "Orchard.Demo",
                template: "Home/Index",
                defaults: new { controller = "Home", action = "Index" }
            );

            routes.MapAreaRoute(
                name: "Display",
                areaName: "Orchard.Demo",
                template: "Home/Display/{contentItemId}",
                defaults: new { controller = "Home", action = "Display" }
            );

            routes.MapAreaRoute(
                name: "Error",
                areaName: "Orchard.Demo",
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
        }
    }
}
