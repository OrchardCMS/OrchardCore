using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Handlers;
using Orchard.Contents.Services;
using Orchard.ContentTypes.Editors;
using Orchard.Data.Migration;
using Orchard.Lists.Drivers;
using Orchard.Lists.Indexes;
using Orchard.Lists.Models;
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
            services.AddSingleton<ContentPart, ListPart>();
            services.AddScoped<IContentPartHandler, ListPartHandler>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, ListPartSettingsDisplayDriver>();
            services.AddScoped<IDataMigration, Migrations>();
        }
    }
}
