using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Menu.Controllers;
using OrchardCore.Menu.Drivers;
using OrchardCore.Menu.Handlers;
using OrchardCore.Menu.Models;
using OrchardCore.Menu.Settings;
using OrchardCore.Menu.TagHelpers;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Menu
{
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddDataMigration<Migrations>();
            services.AddScoped<IShapeTableProvider, MenuShapes>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            // MenuPart
            services.AddScoped<IContentHandler, MenuContentHandler>();
            services.AddContentPart<MenuPart>()
                .UseDisplayDriver<MenuPartDisplayDriver>();

            services.AddContentPart<MenuItemsListPart>();

            // LinkMenuItemPart
            services.AddContentPart<LinkMenuItemPart>()
                .UseDisplayDriver<LinkMenuItemPartDisplayDriver>();

            // ContentMenuItemPart
            services.AddContentPart<ContentMenuItemPart>()
                .UseDisplayDriver<ContentMenuItemPartDisplayDriver>();

            // HtmlMenuItemPart
            services.AddContentPart<HtmlMenuItemPart>()
                .UseDisplayDriver<HtmlMenuItemPartDisplayDriver>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, HtmlMenuItemPartSettingsDisplayDriver>();

            services.AddTagHelpers<MenuTagHelper>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var adminControllerName = typeof(AdminController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "MenuCreate",
                areaName: "OrchardCore.Menu",
                pattern: _adminOptions.AdminUrlPrefix + "/Menu/Create/{id}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Create) }
            );
            routes.MapAreaControllerRoute(
                name: "MenuDelete",
                areaName: "OrchardCore.Menu",
                pattern: _adminOptions.AdminUrlPrefix + "/Menu/Delete",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Delete) }
            );
            routes.MapAreaControllerRoute(
                name: "MenuEdit",
                areaName: "OrchardCore.Menu",
                pattern: _adminOptions.AdminUrlPrefix + "/Menu/Edit",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Edit) }
            );
        }
    }
}
