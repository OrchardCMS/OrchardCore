using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Contents.Services;
using Orchard.ContentTypes.Editors;
using Orchard.Data.Migration;
using Orchard.Lists.Drivers;
using Orchard.Lists.Indexes;
using Orchard.Lists.Services;
using Orchard.Lists.Settings;
using YesSql.Core.Indexes;

namespace Orchard.Lists
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IIndexProvider, ContainedPartIndexProvider>();
            services.AddScoped<IContentDisplayDriver, ContainedPartDisplayDriver>();
            services.AddTransient<IContentAdminFilter, ListPartContentAdminFilter>();

            // List Part
            services.AddScoped<IContentPartDisplayDriver, ListPartDisplayDriver>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, ListPartSettingsDisplayDriver>();
            services.AddScoped<IContentPartDriver, ListPartDriver>();
            services.AddScoped<IContentHandler, ListPartDriver>();
            services.AddScoped<IDataMigration, Migrations>();
        }
    }
}
