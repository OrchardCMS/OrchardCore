using System;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Autoroute.Core.Indexes;
using OrchardCore.Autoroute.Core.Services;
using OrchardCore.Autoroute.Drivers;
using OrchardCore.Autoroute.Handlers;
using OrchardCore.Autoroute.Indexing;
using OrchardCore.Autoroute.Models;
using OrchardCore.Autoroute.Routing;
using OrchardCore.Autoroute.Settings;
using OrchardCore.Autoroute.Sitemaps;
using OrchardCore.Autoroute.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Indexing;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Routing;
using OrchardCore.Security.Permissions;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Autoroute
{
    public class Startup : StartupBase
    {
        public override int ConfigureOrder => -100;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<TemplateOptions>(o =>
            {
                o.MemberAccessStrategy.Register<AutoroutePartViewModel>();

                o.MemberAccessStrategy.Register<LiquidContentAccessor, LiquidPropertyAccessor>("Slug", (obj, context) =>
                {
                    var liquidTemplateContext = (LiquidTemplateContext)context;

                    return new LiquidPropertyAccessor(liquidTemplateContext, async (slug, context) =>
                    {
                        var autorouteEntries = context.Services.GetRequiredService<IAutorouteEntries>();
                        var contentManager = context.Services.GetRequiredService<IContentManager>();

                        if (!slug.StartsWith('/'))
                        {
                            slug = "/" + slug;
                        }

                        (var found, var entry) = await autorouteEntries.TryGetEntryByPathAsync(slug);

                        if (found)
                        {
                            return FluidValue.Create(await contentManager.GetAsync(entry.ContentItemId, entry.JsonPath), context.Options);
                        }

                        return NilValue.Instance;
                    });
                });
            });

            // Autoroute Part
            services.AddContentPart<AutoroutePart>()
                .UseDisplayDriver<AutoroutePartDisplayDriver>()
                .AddHandler<AutoroutePartHandler>();

            services.AddScoped<IContentHandler, DefaultRouteContentHandler>();
            services.AddScoped<IContentHandler, AutorouteContentHandler>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, AutoroutePartSettingsDisplayDriver>();
            services.AddScoped<IContentPartIndexHandler, AutoroutePartIndexHandler>();

            services.AddScoped<AutoroutePartIndexProvider>();
            services.AddScoped<IScopedIndexProvider>(sp => sp.GetRequiredService<AutoroutePartIndexProvider>());
            services.AddScoped<IContentHandler>(sp => sp.GetRequiredService<AutoroutePartIndexProvider>());

            services.AddDataMigration<Migrations>();
            services.AddSingleton<IAutorouteEntries, AutorouteEntries>();
            services.AddScoped<IContentHandleProvider, AutorouteHandleProvider>();

            services.Configure<GraphQLContentOptions>(options =>
            {
                options.ConfigurePart<AutoroutePart>(partOptions =>
                {
                    partOptions.Collapse = true;
                });
            });

            services.AddSingleton<AutorouteTransformer>();
            services.AddSingleton<IShellRouteValuesAddressScheme, AutorouteValuesAddressScheme>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            // The 1st segment prevents the transformer to be executed for the home.
            routes.MapDynamicControllerRoute<AutorouteTransformer>("/{any}/{**slug}");
        }
    }

    [RequireFeatures("OrchardCore.Sitemaps")]
    public class SitemapStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IRouteableContentTypeProvider, AutorouteContentTypeProvider>();
        }
    }
}
