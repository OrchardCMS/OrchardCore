using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentTypes.Editors;
using Orchard.Data.Migration;
using Orchard.Alias.Drivers;
using Orchard.Alias.Handlers;
using Orchard.Alias.Indexes;
using Orchard.Alias.Indexing;
using Orchard.Alias.Models;
using Orchard.Alias.Services;
using Orchard.Alias.Settings;
using Orchard.Indexing;
using YesSql.Core.Indexes;
using Orchard.Tokens;

namespace Orchard.Alias
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IIndexProvider, AliasPartIndexProvider>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IContentAliasProvider, AliasPartContentAliasProvider>();

            // Identity Part
            services.AddScoped<IContentPartDisplayDriver, AliasPartDisplayDriver>();
            services.AddScoped<ContentPart, AliasPart>();
            services.AddScoped<IContentPartHandler, AliasPartHandler>();
            services.AddScoped<IContentPartIndexHandler, AliasPartIndexHandler>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, AliasPartSettingsDisplayDriver>();

            services.AddNullTokenizer();

        }
    }
}
