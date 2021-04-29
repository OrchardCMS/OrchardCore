using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Security.Permissions;
using OrchardCore.Sitemaps.Builders;
using OrchardCore.Sitemaps.Cache;
using OrchardCore.Sitemaps.Controllers;
using OrchardCore.Sitemaps.Drivers;
using OrchardCore.Sitemaps.Handlers;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Routing;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps
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
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions>();

            services.Configure<SitemapsOptions>(options =>
            {
                if (options.GlobalRouteValues.Count == 0)
                {
                    options.GlobalRouteValues = new RouteValueDictionary
                    {
                        {"Area", "OrchardCore.Sitemaps"},
                        {"Controller", "Sitemap"},
                        {"Action", "Index"}
                    };

                    options.SitemapIdKey = "sitemapId";
                }
            });

            services.AddSingleton<SitemapEntries>();
            services.AddSingleton<ISitemapManager, SitemapManager>();
            services.AddSingleton<IShellRouteValuesAddressScheme, SitemapValuesAddressScheme>();
            services.AddSingleton<SitemapRouteTransformer>();

            services.AddScoped<ISitemapIdGenerator, SitemapIdGenerator>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<ISitemapHelperService, SitemapHelperService>();
            services.AddScoped<IDisplayManager<SitemapSource>, DisplayManager<SitemapSource>>();
            services.AddScoped<ISitemapBuilder, DefaultSitemapBuilder>();
            services.AddScoped<ISitemapTypeBuilder, SitemapTypeBuilder>();
            services.AddScoped<ISitemapCacheProvider, DefaultSitemapCacheProvider>();
            services.AddScoped<ISitemapUpdateHandler, DefaultSitemapUpdateHandler>();
            services.AddScoped<ISitemapTypeUpdateHandler, SitemapTypeUpdateHandler>();
            services.AddScoped<ISitemapTypeBuilder, SitemapIndexTypeBuilder>();
            services.AddScoped<ISitemapTypeUpdateHandler, SitemapIndexTypeUpdateHandler>();
            services.AddScoped<ISitemapModifiedDateProvider, DefaultSitemapModifiedDateProvider>();
            services.AddScoped<IRouteableContentTypeCoordinator, DefaultRouteableContentTypeCoordinator>();

            // Sitemap Part.
            services.AddContentPart<SitemapPart>()
                .UseDisplayDriver<SitemapPartDisplayDriver>()
                .AddHandler<SitemapPartHandler>();

            // Custom sitemap path.
            services.AddScoped<ISitemapSourceBuilder, CustomPathSitemapSourceBuilder>();
            services.AddScoped<ISitemapSourceUpdateHandler, CustomPathSitemapSourceUpdateHandler>();
            services.AddScoped<ISitemapSourceModifiedDateProvider, CustomPathSitemapSourceModifiedDateProvider>();
            services.AddScoped<IDisplayDriver<SitemapSource>, CustomPathSitemapSourceDriver>();
            services.AddScoped<ISitemapSourceFactory, SitemapSourceFactory<CustomPathSitemapSource>>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var adminControllerName = typeof(AdminController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "SitemapsList",
                areaName: "OrchardCore.Sitemaps",
                pattern: _adminOptions.AdminUrlPrefix + "/Sitemaps/List",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.List) }
            );

            routes.MapAreaControllerRoute(
                name: "SitemapsDisplay",
                areaName: "OrchardCore.Sitemaps",
                pattern: _adminOptions.AdminUrlPrefix + "/Sitemaps/Display/{id}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Display) }
            );

            routes.MapAreaControllerRoute(
                name: "SitemapsCreate",
                areaName: "OrchardCore.Sitemaps",
                pattern: _adminOptions.AdminUrlPrefix + "/Sitemaps/Create",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Create) }
            );

            routes.MapAreaControllerRoute(
                name: "SitemapsEdit",
                areaName: "OrchardCore.Sitemaps",
                pattern: _adminOptions.AdminUrlPrefix + "/Sitemaps/Edit/{sitemapId}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Edit) }
            );

            routes.MapAreaControllerRoute(
                name: "SitemapsDelete",
                areaName: "OrchardCore.Sitemaps",
                pattern: _adminOptions.AdminUrlPrefix + "/Sitemaps/Delete/{sitemapId}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Delete) }
            );

            routes.MapAreaControllerRoute(
                name: "SitemapsToggle",
                areaName: "OrchardCore.Sitemaps",
                pattern: _adminOptions.AdminUrlPrefix + "/Sitemaps/Toggle/{sitemapId}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Toggle) }
            );

            var sitemapIndexController = typeof(SitemapIndexController).ControllerName();

            routes.MapAreaControllerRoute(
                 name: "SitemapIndexesList",
                 areaName: "OrchardCore.Sitemaps",
                 pattern: _adminOptions.AdminUrlPrefix + "/SitemapIndexes/List",
                 defaults: new { controller = sitemapIndexController, action = nameof(SitemapIndexController.List) }
            );

            routes.MapAreaControllerRoute(
                 name: "SitemapIndexesCreate",
                 areaName: "OrchardCore.Sitemaps",
                 pattern: _adminOptions.AdminUrlPrefix + "/SitemapIndexes/Create",
                 defaults: new { controller = sitemapIndexController, action = nameof(SitemapIndexController.Create) }
            );

            routes.MapAreaControllerRoute(
                 name: "SitemapIndexesEdit",
                 areaName: "OrchardCore.Sitemaps",
                 pattern: _adminOptions.AdminUrlPrefix + "/SitemapIndexes/Edit/{sitemapId}",
                 defaults: new { controller = sitemapIndexController, action = nameof(SitemapIndexController.Edit) }
            );

            routes.MapAreaControllerRoute(
                 name: "SitemapIndexesDelete",
                 areaName: "OrchardCore.Sitemaps",
                 pattern: _adminOptions.AdminUrlPrefix + "/SitemapIndexes/Delete/{sitemapId}",
                 defaults: new { controller = sitemapIndexController, action = nameof(SitemapIndexController.Delete) }
            );

            routes.MapAreaControllerRoute(
                 name: "SitemapIndexesToggle",
                 areaName: "OrchardCore.Sitemaps",
                 pattern: _adminOptions.AdminUrlPrefix + "/SitemapIndexes/Toggle/{sitemapId}",
                 defaults: new { controller = sitemapIndexController, action = nameof(SitemapIndexController.Toggle) }
            );

            var sourceController = typeof(SourceController).ControllerName();

            routes.MapAreaControllerRoute(
                 name: "SitemapsSourceCreate",
                 areaName: "OrchardCore.Sitemaps",
                 pattern: _adminOptions.AdminUrlPrefix + "/SitemapSource/Create/{sitemapId}/{sourceType}",
                 defaults: new { controller = sourceController, action = nameof(SourceController.Create) }
            );

            routes.MapAreaControllerRoute(
                 name: "SitemapsSourceEdit",
                 areaName: "OrchardCore.Sitemaps",
                 pattern: _adminOptions.AdminUrlPrefix + "/SitemapSource/Edit/{sitemapId}/{sourceId}",
                 defaults: new { controller = sourceController, action = nameof(SourceController.Edit) }
            );

            routes.MapAreaControllerRoute(
                 name: "SitemapsSourceDelete",
                 areaName: "OrchardCore.Sitemaps",
                 pattern: _adminOptions.AdminUrlPrefix + "/SitemapSource/Delete/{sitemapId}/{sourceId}",
                 defaults: new { controller = sourceController, action = nameof(SourceController.Delete) }
            );


            var sitemapCacheController = typeof(SitemapCacheController).ControllerName();

            routes.MapAreaControllerRoute(
                 name: "SitemapsCacheList",
                 areaName: "OrchardCore.Sitemaps",
                 pattern: _adminOptions.AdminUrlPrefix + "/SitemapsCache/List",
                 defaults: new { controller = sitemapCacheController, action = nameof(SitemapCacheController.List) }
            );

            routes.MapAreaControllerRoute(
                 name: "SitemapsCachePurgeAll",
                 areaName: "OrchardCore.Sitemaps",
                 pattern: _adminOptions.AdminUrlPrefix + "/SitemapsCache/PurgeAll",
                 defaults: new { controller = sitemapCacheController, action = nameof(SitemapCacheController.PurgeAll) }
            );

            routes.MapAreaControllerRoute(
                 name: "SitemapsCachePurge",
                 areaName: "OrchardCore.Sitemaps",
                 pattern: _adminOptions.AdminUrlPrefix + "/SitemapsCache/Purge/{cacheFileName}",
                 defaults: new { controller = sitemapCacheController, action = nameof(SitemapCacheController.Purge) }
            );

            routes.MapDynamicControllerRoute<SitemapRouteTransformer>("/{**sitemap}");
        }
    }

    [Feature("OrchardCore.Sitemaps.RazorPages")]
    public class SitemapsRazorPagesStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions<SitemapsRazorPagesOptions>();
            services.AddScoped<IRouteableContentTypeProvider, RazorPagesContentTypeProvider>();
        }
    }

    [Feature("OrchardCore.Sitemaps.Cleanup")]
    public class SitemapsCleanupStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IBackgroundTask, SitemapCacheBackgroundTask>();
        }
    }
}
