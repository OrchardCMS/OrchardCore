using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Title;
using OrchardCore.Modules;
using OrchardCore.Seo.Drivers;
using OrchardCore.Seo.Models;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.Seo
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDataMigration, Migrations>();

            services.AddContentPart<SeoMetaPart>()
                .UseDisplayDriver<SeoMetaPartDisplay>()
                .AddHandler<SeoMetaPartHandler>();

            services.AddScoped<IContentDisplayDriver, SeoContentDriver>();

            services.RemoveAll<IPageTitleBuilder>();

            services.AddScoped<IPageTitleBuilder, SeoPageTitleBuilder>();

            // This must be last, and the module dependant on Contents so this runs after the part handlers.
            services.AddScoped<IContentHandler, SeoMetaSettingsHandler>();
        }
    }
}
