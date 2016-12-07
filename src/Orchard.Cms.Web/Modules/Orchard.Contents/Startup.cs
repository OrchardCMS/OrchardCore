using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Handlers;
using Orchard.Contents.Drivers;
using Orchard.Contents.Handlers;
using Orchard.Contents.Indexing;
using Orchard.Contents.Recipes;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Navigation;
using Orchard.Indexing;
using Orchard.Recipes;
using Orchard.Security.Permissions;

namespace Orchard.Contents
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddContentManagement();
            services.AddContentManagementDisplay();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IShapeTableProvider, Shapes>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IContentDisplayDriver, ContentsDriver>();
            services.AddScoped<IContentHandler, ContentsHandler>();
            services.AddRecipeExecutionStep<ContentStep>();

            services.AddScoped<IContentItemIndexHandler, AspectsContentIndexHandler>();
            services.AddScoped<IContentItemIndexHandler, DefaultContentIndexHandler>();
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "DisplayContent",
                areaName: "Orchard.Contents",
                template: "Contents/Item/Display/{contentItemId}",
                defaults: new {controller = "Item", action = "Display" }
            );

            routes.MapAreaRoute(
                name: "PreviewContent",
                areaName: "Orchard.Contents",
                template: "Contents/Item/Preview/{contentItemId}",
                defaults: new { controller = "Item", action = "Preview" }
            );
        }
    }
}