using Fluid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Flows.Drivers;
using OrchardCore.Flows.Handlers;
using OrchardCore.Flows.Indexing;
using OrchardCore.Flows.Models;
using OrchardCore.Flows.Settings;
using OrchardCore.Flows.ViewModels;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Flows;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<TemplateOptions>(o =>
        {
            o.MemberAccessStrategy.Register<BagPartViewModel>();
            o.MemberAccessStrategy.Register<FlowPartViewModel>();
            o.MemberAccessStrategy.Register<FlowMetadata>();
            o.MemberAccessStrategy.Register<FlowPart>();
        });

        // Flow Part
        services.AddContentPart<FlowPart>()
            .UseDisplayDriver<FlowPartDisplayDriver>();
        services.AddScoped<IContentTypePartDefinitionDisplayDriver, FlowPartSettingsDisplayDriver>();
        services.AddScoped<IContentPartIndexHandler, FlowPartIndexHandler>();

        services.AddScoped<IContentDisplayDriver, FlowMetadataDisplayDriver>();

        // Bag Part
        services.AddContentPart<BagPart>()
            .UseDisplayDriver<BagPartDisplayDriver>()
            .AddHandler<BagPartHandler>();
        services.AddScoped<IContentTypePartDefinitionDisplayDriver, BagPartSettingsDisplayDriver>();
        services.AddScoped<IContentPartIndexHandler, BagPartIndexHandler>();

        services.AddContentPart<FlowMetadata>();

        services.AddDataMigration<Migrations>();

        services.AddTransient<IConfigureOptions<ResourceManagementOptions>, ResourceManagementOptionsConfiguration>();
    }
}
