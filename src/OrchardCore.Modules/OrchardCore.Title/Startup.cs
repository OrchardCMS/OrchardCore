using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Title.Drivers;
using OrchardCore.Title.Handlers;
using OrchardCore.Title.Indexing;
using OrchardCore.Title.Models;
using OrchardCore.Title.Settings;
using OrchardCore.Title.ViewModels;

namespace OrchardCore.Title
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<TemplateOptions>(o =>
            {
                o.MemberAccessStrategy.Register<TitlePartViewModel>();
                o.MemberAccessStrategy.Register<TitlePartSettingsViewModel>();
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
}
