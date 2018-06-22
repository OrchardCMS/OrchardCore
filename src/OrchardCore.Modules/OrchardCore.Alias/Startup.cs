using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Alias.Drivers;
using OrchardCore.Alias.Handlers;
using OrchardCore.Alias.Indexes;
using OrchardCore.Alias.Indexing;
using OrchardCore.Alias.Liquid;
using OrchardCore.Alias.Models;
using OrchardCore.Alias.Services;
using OrchardCore.Alias.Settings;
using OrchardCore.Alias.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Indexing;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using YesSql.Indexes;

namespace OrchardCore.Alias
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<AliasPartViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IIndexProvider, AliasPartIndexProvider>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IContentAliasProvider, AliasPartContentAliasProvider>();

            // Identity Part
            services.AddScoped<IContentPartDisplayDriver, AliasPartDisplayDriver>();
            services.AddSingleton<ContentPart, AliasPart>();
            services.AddScoped<IContentPartHandler, AliasPartHandler>();
            services.AddScoped<IContentPartIndexHandler, AliasPartIndexHandler>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, AliasPartSettingsDisplayDriver>();

            services.AddScoped<ILiquidTemplateEventHandler, ContentAliasLiquidTemplateEventHandler>();
        }
    }
}
