using System;
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
using OrchardCore.Sitemaps.SitemapNodes;

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

            services.AddSingleton<SitemapEntries>();

            services.AddScoped<ISitemapIdGenerator, SitemapIdGenerator>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<ISitemapService, SitemapService>();
            services.AddScoped<IDisplayManager<SitemapNode>, DisplayManager<SitemapNode>>();
            services.AddScoped<ISitemapBuilder, DefaultSitemapBuilder>();

            // Sitemap Index node
            services.AddScoped<ISitemapNodeProviderFactory, SitemapNodeProviderFactory<SitemapIndexNode>>();
            services.AddSingleton<ISitemapNodeBuilder, SitemapIndexNodeBuilder>();
            services.AddScoped<IDisplayDriver<SitemapNode>, SitemapIndexNodeDriver>();

            // Sitemap Part.
            services.AddScoped<IContentPartDisplayDriver, SitemapPartDisplay>();
            services.AddContentPart<SitemapPart>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                 name: SitemapRouteConstraint.RouteKey,
                 areaName: "OrchardCore.Sitemap",
                 pattern: "{*sitemap}",
                 constraints: new { sitemaps = new SitemapRouteConstraint() },
                 defaults: new { controller = "Sitemap", action = "Index" }
             );

            var sitemapSetService = serviceProvider.GetService<ISitemapService>();
            var document = sitemapSetService.LoadSitemapDocumentAsync().GetAwaiter().GetResult();
            sitemapSetService.BuildAllSitemapRouteEntries(document);
        }
    }
}
