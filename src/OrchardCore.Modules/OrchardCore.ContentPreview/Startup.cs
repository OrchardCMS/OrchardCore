using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentPreview.Drivers;
using OrchardCore.ContentPreview.Handlers;
using OrchardCore.ContentPreview.Models;
using OrchardCore.ContentPreview.Settings;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;

namespace OrchardCore.ContentPreview;

public sealed class Startup : Modules.StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();

        services.AddScoped<IContentDisplayDriver, ContentPreviewDriver>();

        // Preview Part
        services.AddContentPart<PreviewPart>()
            .AddHandler<PreviewPartHandler>();

        services.AddDataMigration<Migrations>();
        services.AddScoped<IContentTypePartDefinitionDisplayDriver, PreviewPartSettingsDisplayDriver>();
        services.AddSingleton<IStartupFilter, PreviewStartupFilter>();
    }
}
