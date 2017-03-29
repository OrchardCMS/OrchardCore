using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Handlers;
using Orchard.Contents.Services;
using Orchard.ContentTypes.Editors;
using Orchard.Core.XmlRpc;
using Orchard.Data.Migration;
using Orchard.Feeds;
using Orchard.Lists.Drivers;
using Orchard.Lists.Feeds;
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

            // Feeds
            // TODO: Create feature
            services.AddScoped<IFeedQueryProvider, ListFeedQuery>();
            services.AddScoped<IContentPartDisplayDriver, ListPartFeedDisplayDriver>();
            services.AddScoped<IContentPartHandler, ListPartFeedHandler>();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "ListFeed",
                areaName: "Orchard.Feeds",
                template: "Contents/Lists/{contentItemId}/rss",
                defaults: new { controller = "Feed", action = "Index", format = "rss"}
            );
        }
    }
}
