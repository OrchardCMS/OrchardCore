using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Sitemaps.Builders;
using OrchardCore.Sitemaps.Drivers;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Routing;
using OrchardCore.Sitemaps.Services;
using OrchardCore.Sitemaps.Sitemaps;

namespace OrchardCore.Sitemaps
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddIdGeneration();

            //TODO add AddressScheme
            services.Configure<SitemapOptions>(options =>
            {
                if (options.GlobalRouteValues.Count == 0)
                {
                    options.GlobalRouteValues = new RouteValueDictionary
                    {
                        {"Area", "OrchardCore.Sitemaps"},
                        {"Controller", "Sitemaps"},
                        {"Action", "Index"}
                    };

                    options.SitemapIdKey = "sitemapId";
                }
            });

            services.AddSingleton<SitemapsTransformer>();
            services.AddSingleton<SitemapEntries>();

            services.AddScoped<ISitemapIdGenerator, SitemapIdGenerator>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<ISitemapManager, SitemapManager>();
            services.AddScoped<IDisplayManager<Sitemap>, DisplayManager<Sitemap>>();

            // Sitemap Index node
            services.AddScoped<ISitemapProviderFactory, SitemapProviderFactory<SitemapIndex>>();
            services.AddSingleton<ISitemapBuilder, SitemapIndexBuilder>();
            services.AddScoped<IDisplayDriver<Sitemap>, SitemapIndexDriver>();

            // Sitemap Part.
            services.AddScoped<IContentPartDisplayDriver, SitemapPartDisplay>();
            services.AddContentPart<SitemapPart>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                   name: "Sitemaps",
                   areaName: "OrchardCore.Sitemaps",
                   pattern: "{sitemapId}",
                   defaults: new { controller = "Sitemap", action = "Index" }
               );

            routes.MapDynamicControllerRoute<SitemapsTransformer>("/{**sitemap}");
            var sitemapManager = serviceProvider.GetService<ISitemapManager>();
            sitemapManager.BuildAllSitemapRouteEntriesAsync().GetAwaiter().GetResult();
        }
    }
}
