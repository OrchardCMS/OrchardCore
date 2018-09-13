using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Title.Drivers;
using OrchardCore.Title.Indexing;
using OrchardCore.Title.Model;
using OrchardCore.Title.ViewModels;

namespace OrchardCore.Title
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<TitlePartViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            // Title Part
            services.AddScoped<IContentPartDisplayDriver, TitlePartDisplay>();
            services.AddSingleton<ContentPart, TitlePart>();
            services.AddScoped<IContentPartIndexHandler, TitlePartIndexHandler>();

            services.AddScoped<IDataMigration, Migrations>();
        }
    }
}
