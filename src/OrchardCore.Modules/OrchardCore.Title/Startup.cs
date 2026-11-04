using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Indexing;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Title.Drivers;
using OrchardCore.Title.Handlers;
using OrchardCore.Title.Indexing;
using OrchardCore.Title.Models;
using OrchardCore.Title.Settings;
using OrchardCore.Title.ViewModels;

namespace OrchardCore.Title;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<TemplateOptionsBuilder>(o =>
        {
            o.ConfigureMemberAccess(strategy =>
            {
                strategy.Register<TitlePartViewModel>();
                strategy.Register<TitlePartSettingsViewModel>();
            });
        });

        // Title Part
        services.AddContentPart<TitlePart>()
            .UseDisplayDriver<TitlePartDisplayDriver>()
            .AddHandler<TitlePartHandler>();

        services.AddScoped<IContentPartIndexHandler, TitlePartIndexHandler>();
        services.AddScoped<IContentTypePartDefinitionDisplayDriver, TitlePartSettingsDisplayDriver>();

        services.AddDataMigration<Migrations>();
    }
}
