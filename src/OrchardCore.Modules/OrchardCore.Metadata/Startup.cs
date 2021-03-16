using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Metadata.Drivers;
using OrchardCore.Metadata.Fields;
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

            services.AddSingleton<ContentPart, SeoMetadataPart >();
            services.AddSingleton<ContentPart, SocialMetadataPart>();
            services.AddScoped<IDataMigration, Migrations>();
        }
    }
}
