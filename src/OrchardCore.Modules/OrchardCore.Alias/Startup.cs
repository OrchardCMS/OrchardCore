using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Alias.Drivers;
using OrchardCore.Alias.Handlers;
using OrchardCore.Alias.Indexes;
using OrchardCore.Alias.Indexing;
using OrchardCore.Alias.Models;
using OrchardCore.Alias.Services;
using OrchardCore.Alias.Settings;
using OrchardCore.Alias.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Indexing;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.Alias
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<TemplateOptions>(o =>
            {
                o.MemberAccessStrategy.Register<AliasPartViewModel>();

                o.MemberAccessStrategy.Register<LiquidContentAccessor, LiquidPropertyAccessor>("Alias", (obj, context) =>
                {
                    var liquidTemplateContext = (LiquidTemplateContext)context;

                    return new LiquidPropertyAccessor(liquidTemplateContext, async (alias, context) =>
                    {
                        var session = context.Services.GetRequiredService<ISession>();

                        var aliasPartIndex = await session.Query<ContentItem, AliasPartIndex>(x => x.Alias == alias.ToLowerInvariant()).FirstOrDefaultAsync();
                        var contentItemId = aliasPartIndex?.ContentItemId;

                        if (contentItemId == null)
                        {
                            return NilValue.Instance;
                        }

                        var contentManager = context.Services.GetRequiredService<IContentManager>();

                        return FluidValue.Create(await contentManager.GetAsync(contentItemId), context.Options);
                    });
                });
            });
            services.AddScoped<IScopedIndexProvider, AliasPartIndexProvider>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IContentHandleProvider, AliasPartContentHandleProvider>();

            // Identity Part
            services.AddContentPart<AliasPart>()
                .UseDisplayDriver<AliasPartDisplayDriver>()
                .AddHandler<AliasPartHandler>();

            services.AddScoped<IContentPartIndexHandler, AliasPartIndexHandler>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, AliasPartSettingsDisplayDriver>();
        }
    }
}
