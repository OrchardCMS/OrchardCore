using System;
using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Metadata.Drivers;
using OrchardCore.Metadata.Fields;
using OrchardCore.Metadata.Handlers;
using OrchardCore.Metadata.Models;
using OrchardCore.Metadata.Settings;
using OrchardCore.Modules;

namespace OrchardCore.Metadata
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<MetadataTextField>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ContentField, MetadataTextField>();
            services.AddScoped<IContentFieldDisplayDriver, MetadataTextFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, MetadataTextFieldSettingsDriver>();

            services.AddScoped<IContentPartDisplayDriver, MetadataPartDisplayDriver>();
            services.AddSingleton<ContentPart, MetadataPart>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, MetadataPartSettingsDisplayDriver>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IContentPartHandler, MetadataPartHandler>();
        }
    }
}