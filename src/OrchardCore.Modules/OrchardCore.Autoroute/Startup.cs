using System;
using System.Linq;
using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Autoroute.Drivers;
using OrchardCore.Autoroute.Handlers;
using OrchardCore.Autoroute.Indexing;
using OrchardCore.Autoroute.Liquid;
using OrchardCore.Autoroute.Model;
using OrchardCore.Autoroute.Routing;
using OrchardCore.Autoroute.Services;
using OrchardCore.Autoroute.Settings;
using OrchardCore.Autoroute.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Records;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Indexing;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;
using YesSql;
using YesSql.Indexes;

namespace OrchardCore.Autoroute
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<AutoroutePartViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            // Autoroute Part
            services.AddScoped<IContentPartDisplayDriver, AutoroutePartDisplay>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddSingleton<ContentPart, AutoroutePart>();
            services.AddScoped<IContentPartHandler, AutoroutePartHandler>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, AutoroutePartSettingsDisplayDriver>();
            services.AddScoped<IContentPartIndexHandler, AutoroutePartIndexHandler>();

            services.AddSingleton<IIndexProvider, AutoroutePartIndexProvider>();
            services.AddScoped<IDataMigration, Migrations>();

            services.AddSingleton<IAutorouteEntries, AutorouteEntries>();
            services.AddScoped<IContentAliasProvider, AutorouteAliasProvider>();

            services.AddScoped<ILiquidTemplateEventHandler, ContentAutorouteLiquidTemplateEventHandler>();

            services.Configure<GraphQLContentOptions>(options =>
            {
                options.ConfigurePart<AutoroutePart>(partOptions =>
                {
                    partOptions.Collapse = true;
                });
            });
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var entries = serviceProvider.GetRequiredService<IAutorouteEntries>();
            var session = serviceProvider.GetRequiredService<ISession>();
            var autoroutes = session.QueryIndex<AutoroutePartIndex>(o => o.Published).ListAsync().GetAwaiter().GetResult();

            entries.AddEntries(autoroutes.Select(x => new AutorouteEntry { ContentItemId = x.ContentItemId, Path = x.Path }));

            var autorouteRoute = new AutorouteRoute(entries, routes.DefaultHandler);

            routes.Routes.Insert(0, autorouteRoute);
        }
    }
}
