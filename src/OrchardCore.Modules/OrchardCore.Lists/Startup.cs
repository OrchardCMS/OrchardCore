using System;
using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.AdminMenu.Services;
using OrchardCore.ContentLocalization.Handlers;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Feeds;
using OrchardCore.Indexing;
using OrchardCore.Liquid;
using OrchardCore.Lists.AdminNodes;
using OrchardCore.Lists.Drivers;
using OrchardCore.Lists.Feeds;
using OrchardCore.Lists.Handlers;
using OrchardCore.Lists.Indexes;
using OrchardCore.Lists.Liquid;
using OrchardCore.Lists.Models;
using OrchardCore.Lists.Services;
using OrchardCore.Lists.Settings;
using OrchardCore.Lists.ViewModels;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace OrchardCore.Lists
{
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;


        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<TemplateOptions>(o =>
            {
                o.MemberAccessStrategy.Register<ListPartViewModel>();
            })
            .AddLiquidFilter<ListCountFilter>("list_count")
            .AddLiquidFilter<ListItemsFilter>("list_items")
            .AddLiquidFilter<ContainerFilter>("container");

            services.AddIndexProvider<ContainedPartIndexProvider>();
            services.AddScoped<IContentDisplayDriver, ContainedPartDisplayDriver>();
            services.AddScoped<IContentHandler, ContainedPartHandler>();
            services.AddContentPart<ContainedPart>();
            services.AddScoped<IContentsAdminListFilter, ListPartContentsAdminListFilter>();
            services.AddScoped<IDisplayDriver<ContentOptionsViewModel>, ListPartContentsAdminListDisplayDriver>();

            // List Part
            services.AddContentPart<ListPart>()
                .UseDisplayDriver<ListPartDisplayDriver>()
                .AddHandler<ListPartHandler>();

            services.AddScoped<IContentTypePartDefinitionDisplayDriver, ListPartSettingsDisplayDriver>();
            services.AddDataMigration<Migrations>();
            services.AddScoped<IContentItemIndexHandler, ContainedPartContentIndexHandler>();
            services.AddScoped<IContainerService, ContainerService>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "ListOrder",
                areaName: "OrchardCore.Lists",
                pattern: _adminOptions.AdminUrlPrefix + "/Lists/Order/{containerId?}",
                defaults: new { controller = "Order", action = "UpdateContentItemOrders" }
            );
        }
    }

    [RequireFeatures("OrchardCore.AdminMenu")]
    public class AdminMenuStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IAdminNodeProviderFactory>(new AdminNodeProviderFactory<ListsAdminNode>());
            services.AddScoped<IAdminNodeNavigationBuilder, ListsAdminNodeNavigationBuilder>();
            services.AddScoped<IDisplayDriver<MenuItem>, ListsAdminNodeDriver>();
        }
    }

    [RequireFeatures("OrchardCore.ContentLocalization")]
    public class ContentLocalizationStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentLocalizationPartHandler, ContainedPartLocalizationHandler>();
            services.AddScoped<IContentLocalizationPartHandler, ListPartLocalizationHandler>();
            services.AddContentPart<LocalizationPart>()
                .AddHandler<LocalizationContainedPartHandler>();
        }
    }

    [RequireFeatures("OrchardCore.Feeds")]
    public class FeedsStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Feeds
            services.AddScoped<IFeedQueryProvider, ListFeedQuery>();

            services.AddContentPart<ListPart>()
                .UseDisplayDriver<ListPartFeedDisplayDriver>()
                .AddHandler<ListPartFeedHandler>();
        }
        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "ListFeed",
                areaName: "OrchardCore.Feeds",
                pattern: "Contents/Lists/{contentItemId}/rss",
                defaults: new { controller = "Feed", action = "Index", format = "rss" }
            );
        }
    }
}
