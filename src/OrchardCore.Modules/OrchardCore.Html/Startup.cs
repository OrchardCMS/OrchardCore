using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Html.Drivers;
using OrchardCore.Html.Handlers;
using OrchardCore.Html.Indexing;
using OrchardCore.Html.Model;
using OrchardCore.Html.Settings;
using OrchardCore.Html.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
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
            services.AddScoped<IContentPartDisplayDriver, HtmlBodyPartDisplay>();
            services.AddSingleton<ContentPart, HtmlBodyPart>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, HtmlBodyPartSettingsDisplayDriver>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IContentPartIndexHandler, HtmlBodyPartIndexHandler>();
            services.AddScoped<IContentPartHandler, HtmlBodyPartHandler>();
        }
    }
}
