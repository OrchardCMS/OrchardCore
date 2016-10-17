using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentTypes.Editors;
using Orchard.ContentTypes.Services;
using Orchard.Environment.Navigation;
using Orchard.Security.Permissions;

namespace Orchard.ContentTypes
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IContentDefinitionService, ContentDefinitionService>();
            services.AddScoped<IStereotypesProvider, DefaultStereotypesProvider>();
            services.AddScoped<IStereotypeService, StereotypeService>();
            services.AddScoped<IContentDefinitionDisplayHandler, ContentDefinitionDisplayCoordinator>();
            services.AddScoped<IContentDefinitionDisplayManager, DefaultContentDefinitionDisplayManager>();
            services.AddScoped<IContentPartDefinitionDisplayDriver, ContentPartSettingsDisplayDriver>();
            services.AddScoped<IContentTypeDefinitionDisplayDriver, ContentTypeSettingsDisplayDriver>();
            services.AddScoped<IContentTypeDefinitionDisplayDriver, DefaultContentTypeDisplayDriver>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, ContentTypePartSettingsDisplayDriver>();
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "EditField",
                area: "Orchard.ContentTypes",
                template: "Admin/EditField/{id}/{name}",
                controller: "Admin",
                action: "EditField"
            );

            routes.MapAreaRoute(
                name: "EditTypePart",
                area: "Orchard.ContentTypes",
                template: "Admin/Edit/{id}/{name}",
                controller: "Admin",
                action: "EditTypePart"
            );

            routes.MapAreaRoute(
                name: "RemovePart",
                area: "Orchard.ContentTypes",
                template: "Admin/RemovePart/{id}/{name}",
                controller: "Admin",
                action: "RemovePart"
            );
        }
    }
}
