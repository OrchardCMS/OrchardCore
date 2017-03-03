using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Autoroute.Drivers;
using Orchard.Autoroute.Handlers;
using Orchard.Autoroute.Indexing;
using Orchard.Autoroute.Model;
using Orchard.Autoroute.Routing;
using Orchard.Autoroute.Services;
using Orchard.Autoroute.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Records;
using Orchard.ContentTypes.Editors;
using Orchard.Data.Migration;
using Orchard.Indexing;
using Orchard.Security.Permissions;
using Orchard.Tokens;
using YesSql.Core.Indexes;
using YesSql.Core.Services;

namespace Orchard.Autoroute
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Autoroute Part
            services.AddScoped<IContentPartDisplayDriver, AutoroutePartDisplay>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddSingleton<ContentPart, AutoroutePart>();
            services.AddScoped<IContentPartHandler, AutoroutePartHandler>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, AutoroutePartSettingsDisplayDriver>();
            services.AddScoped<IContentPartIndexHandler, AutoroutePartIndexHandler>();

            services.AddScoped<IIndexProvider, AutoroutePartIndexProvider>();
            services.AddScoped<IDataMigration, Migrations>();

            services.AddSingleton<IAutorouteEntries, AutorouteEntries>();
            services.AddScoped<IContentAliasProvider, AutorouteAliasProvider>();

            services.AddNullTokenizer();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var entries = serviceProvider.GetRequiredService<IAutorouteEntries>();
            var session = serviceProvider.GetRequiredService<ISession>();
            var autoroutes = session.QueryIndexAsync<AutoroutePartIndex>().List().GetAwaiter().GetResult();

            entries.AddEntries(autoroutes.Select(x => new AutorouteEntry { ContentItemId = x.ContentItemId, Path = x.Path }));
            
            var autorouteRoute = new AutorouteRoute(entries, routes.DefaultHandler);

            routes.Routes.Insert(0, autorouteRoute);
        }
    }
}
