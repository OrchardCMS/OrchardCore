using OrchardCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Widgets.Drivers;
using OrchardCore.Widgets.Models;
using OrchardCore.Widgets.Settings;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Admin;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;
using OrchardCore.Widgets.Controllers;
using OrchardCore.Mvc.Core.Utilities;

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
            services.AddScoped<IShapeTableProvider,ContentCardShapes>();
            // Widgets List Part
            services.AddScoped<IContentPartDisplayDriver, WidgetsListPartDisplay>();
            services.AddContentPart<WidgetsListPart>();

            services.AddScoped<IContentTypePartDefinitionDisplayDriver, WidgetsListPartSettingsDisplayDriver>();
            services.AddContentPart<WidgetMetadata>();
            services.AddScoped<IDataMigration, Migrations>();
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
