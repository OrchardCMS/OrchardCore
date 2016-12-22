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
using Orchard.Contents.Feeds.Builders;
using Orchard.Contents.Handlers;
using Orchard.Contents.Indexing;
using Orchard.Contents.Recipes;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Navigation;
using Orchard.Feeds;
using Orchard.Indexing;
using Orchard.Recipes;
using Orchard.Scripting;
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

            services.AddScoped<IGlobalMethodProvider, IdGeneratorMethod>();

            // Feeds
            // TODO: Move to feature
            services.AddScoped<IFeedItemBuilder, CommonFeedItemBuilder>();
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "DisplayContentItem",
                areaName: "Orchard.Contents",
                template: "Contents/ContentItems/{contentItemId}",
                defaults: new {controller = "Item", action = "Display" }
            );

            routes.MapAreaRoute(
                name: "PreviewContentItem",
                areaName: "Orchard.Contents",
                template: "Contents/ContentItems/{contentItemId}/Preview",
                defaults: new { controller = "Item", action = "Preview" }
            );

            // Admin
            routes.MapAreaRoute(
                name: "EditContentItem",
                areaName: "Orchard.Contents",
                template: "Admin/Contents/ContentItems/{contentItemId}/Edit",
                defaults: new { controller = "Admin", action = "Edit" }
            );

            routes.MapAreaRoute(
                name: "CreateContentItem",
                areaName: "Orchard.Contents",
                template: "Admin/Contents/ContentTypes/{id}/Create",
                defaults: new { controller = "Admin", action = "Create" }
            );

            routes.MapAreaRoute(
                name: "AdminContentItem",
                areaName: "Orchard.Contents",
                template: "Admin/Contents/ContentItems/{contentItemId}/Display",
                defaults: new { controller = "Admin", action = "Display" }
            );

            routes.MapAreaRoute(
                name: "ListContentItems",
                areaName: "Orchard.Contents",
                template: "Admin/Contents/ContentItems",
                defaults: new { controller = "Admin", action = "List" }
            );


        }
    }
}