using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Autoroute.Drivers;
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
using Orchard.Environment.Shell;
using Orchard.Title.Handlers;
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
            services.AddSingleton<ContentPart, AutoroutePart>();
            services.AddScoped<IContentPartHandler, AutoroutePartHandler>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, AutoroutePartSettingsDisplayDriver>();

            services.AddScoped<IIndexProvider, AutoroutePartIndexProvider>();
            services.AddScoped<IDataMigration, Migrations>();

            services.AddSingleton<IAutorouteEntries, AutorouteEntries>();

            services.AddNullTokenizer();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var entries = serviceProvider.GetRequiredService<IAutorouteEntries>();
            var settings = serviceProvider.GetRequiredService<ShellSettings>();
            var session = serviceProvider.GetRequiredService<ISession>();
            var autoroutes = session.QueryIndexAsync<AutoroutePartIndex>().List().Result;

            entries.AddEntries(autoroutes.Select(x => new AutorouteEntry { ContentItemId = x.ContentItemId, Path = x.Path }));
            
            var autorouteRoute = new AutorouteRoute(settings.RequestUrlPrefix, entries, routes.DefaultHandler);

            routes.Routes.Add(autorouteRoute);
        }
    }
}
