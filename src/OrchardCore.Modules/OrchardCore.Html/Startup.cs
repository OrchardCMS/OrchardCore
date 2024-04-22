using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Html.Drivers;
using OrchardCore.Html.Handlers;
using OrchardCore.Html.Indexing;
using OrchardCore.Html.Models;
using OrchardCore.Html.Settings;
using OrchardCore.Html.ViewModels;
using OrchardCore.Indexing;
using OrchardCore.Modules;

namespace OrchardCore.Html
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<TemplateOptions>(o => o.MemberAccessStrategy.Register<HtmlBodyPartViewModel>());

            // Body Part
            services.AddContentPart<HtmlBodyPart>()
                .UseDisplayDriver<HtmlBodyPartDisplayDriver>()
                .AddHandler<HtmlBodyPartHandler>();

            services.AddScoped<IContentTypePartDefinitionDisplayDriver, HtmlBodyPartSettingsDisplayDriver>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, HtmlBodyPartTrumbowygEditorSettingsDriver>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, HtmlBodyPartMonacoEditorSettingsDriver>();
            services.AddDataMigration<Migrations>();
            services.AddScoped<IContentPartIndexHandler, HtmlBodyPartIndexHandler>();
        }
    }
}
