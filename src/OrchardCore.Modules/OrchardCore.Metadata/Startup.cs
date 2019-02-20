using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Metadata.Drivers;
using OrchardCore.Metadata.Handlers;
using OrchardCore.Metadata.Models;
using OrchardCore.Metadata.Settings;
using OrchardCore.Modules;

namespace OrchardCore.Metadata
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentPartDisplayDriver, MetadataPartDisplayDriver>();
            services.AddSingleton<ContentPart, MetadataPart>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, MetadataPartSettingsDisplayDriver>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IContentPartHandler, MetadataPartHandler>();
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "Home",
                areaName: "OrchardCore.Metadata",
                template: "Home/Index",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }
}