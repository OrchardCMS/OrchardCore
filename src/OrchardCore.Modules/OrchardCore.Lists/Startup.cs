using System;
using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Contents.Services;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Feeds;
using OrchardCore.Lists.Drivers;
using OrchardCore.Lists.Feeds;
using OrchardCore.Lists.Indexes;
using OrchardCore.Lists.Models;
using OrchardCore.Lists.Services;
using OrchardCore.Lists.Settings;
using OrchardCore.Lists.ViewModels;
using OrchardCore.Modules;
using YesSql.Indexes;

namespace OrchardCore.Lists
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<ListPartViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IIndexProvider, ContainedPartIndexProvider>();
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
                areaName: "OrchardCore.Feeds",
                template: "Contents/Lists/{contentItemId}/rss",
                defaults: new { controller = "Feed", action = "Index", format = "rss"}
            );
        }
    }
}
