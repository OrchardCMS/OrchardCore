using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Handlers;
using Orchard.Data.Migration;
using Orchard.Indexing;
using Orchard.MetaWeblog;
using Orchard.Title.Drivers;
using Orchard.Title.Handlers;
using Orchard.Title.Indexing;
using Orchard.Title.Model;

namespace Orchard.Title
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Title Part
            services.AddScoped<IContentPartDisplayDriver, TitlePartDisplay>();
            services.AddSingleton<ContentPart, TitlePart>();
            services.AddScoped<IContentPartHandler, TitlePartHandler>();
            services.AddScoped<IContentPartIndexHandler, TitlePartIndexHandler>();

            services.AddScoped<IDataMigration, Migrations>();
        }
    }

    [OrchardFeature("Orchard.RemotePublishing")]
    public class RemotePublishingStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IMetaWeblogDriver, TitleMetaWeblogDriver>();
        }
    }
}
