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
        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<HtmlBodyPartViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            // Body Part
            services.AddContentPart<HtmlBodyPart>()
                .UseDisplayDriver<HtmlBodyPartDisplayDriver>()
                .AddHandler<HtmlBodyPartHandler>();

            services.AddScoped<IContentTypePartDefinitionDisplayDriver, HtmlBodyPartSettingsDisplayDriver>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, HtmlBodyPartTrumbowygEditorSettingsDriver>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IContentPartIndexHandler, HtmlBodyPartIndexHandler>();
        }
    }
}
