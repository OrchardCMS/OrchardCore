using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Widgets.Controllers;
using OrchardCore.Widgets.Drivers;
using OrchardCore.Widgets.Models;
using OrchardCore.Widgets.Settings;

namespace OrchardCore.Widgets
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
            //Add Widget Card Shapes
            services.AddScoped<IShapeTableProvider, ContentCardShapes>();
            // Widgets List Part
            services.AddContentPart<WidgetsListPart>()
                .UseDisplayDriver<WidgetsListPartDisplayDriver>();

            services.AddScoped<IContentTypePartDefinitionDisplayDriver, WidgetsListPartSettingsDisplayDriver>();
            services.AddContentPart<WidgetMetadata>();
            services.AddDataMigration<Migrations>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "Widgets.BuildEditor",
                areaName: "OrchardCore.Widgets",
                pattern: _adminOptions.AdminUrlPrefix + "/Widgets/BuildEditor",
                defaults: new { controller = typeof(AdminController).ControllerName(), action = nameof(AdminController.BuildEditor) }
            );
        }
    }
}
