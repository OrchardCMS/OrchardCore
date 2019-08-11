using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Flows.Drivers;
using OrchardCore.Flows.Indexing;
using OrchardCore.Flows.Models;
using OrchardCore.Flows.Settings;
using OrchardCore.Flows.ViewModels;
using OrchardCore.Indexing;
using OrchardCore.Modules;

namespace OrchardCore.Flows
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<BagPartViewModel>();
            TemplateContext.GlobalMemberAccessStrategy.Register<FlowPartViewModel>();
            TemplateContext.GlobalMemberAccessStrategy.Register<FlowMetadata>();
            TemplateContext.GlobalMemberAccessStrategy.Register<FlowPart>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            // Flow Part
            services.AddScoped<IContentPartDisplayDriver, FlowPartDisplay>();
            services.AddSingleton<ContentPart, FlowPart>();
            services.AddScoped<IContentDisplayDriver, FlowMetadataDisplay>();

            // Bag Part
            services.AddScoped<IContentPartDisplayDriver, BagPartDisplay>();
            services.AddSingleton<ContentPart, BagPart>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, BagPartSettingsDisplayDriver>();
            services.AddScoped<IContentPartIndexHandler, BagPartIndexHandler>();

            services.AddSingleton<ContentPart, FlowMetadata>();
            services.AddScoped<IDataMigration, Migrations>();
        }
    }
}
