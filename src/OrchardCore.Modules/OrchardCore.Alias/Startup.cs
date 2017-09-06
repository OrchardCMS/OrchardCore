using OrchardCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Alias.Drivers;
using OrchardCore.Alias.Handlers;
using OrchardCore.Alias.Indexes;
using OrchardCore.Alias.Indexing;
using OrchardCore.Alias.Models;
using OrchardCore.Alias.Services;
using OrchardCore.Alias.Settings;
using OrchardCore.Indexing;
using YesSql.Indexes;

namespace OrchardCore.Alias
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IIndexProvider, AliasPartIndexProvider>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IContentAliasProvider, AliasPartContentAliasProvider>();

            // Identity Part
            services.AddScoped<IContentPartDisplayDriver, AliasPartDisplayDriver>();
            services.AddScoped<ContentPart, AliasPart>();
            services.AddScoped<IContentPartHandler, AliasPartHandler>();
            services.AddScoped<IContentPartIndexHandler, AliasPartIndexHandler>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, AliasPartSettingsDisplayDriver>();
        }
    }
}
