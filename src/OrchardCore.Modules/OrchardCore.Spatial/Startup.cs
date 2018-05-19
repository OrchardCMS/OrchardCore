using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Spatial.Drivers;
using OrchardCore.Spatial.Indexing;
using OrchardCore.Spatial.ViewModels;

namespace OrchardCore.Spatial
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<GeoPointPartViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ContentPart, GeoPointPart>();

            services.AddScoped<IContentPartDisplayDriver, GeoPointPartDisplayDriver>();
            services.AddScoped<IContentPartIndexHandler, GeoPointPartIndexHandler>();
            services.AddScoped<IDataMigration, Migrations>();
        }
    }
}
