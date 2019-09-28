using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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
            services.AddScoped<ISitemapSetService, SitemapSetService>();
            services.AddScoped<IDisplayManager<SitemapNode>, DisplayManager<SitemapNode>>();
            services.AddScoped<ISitemapBuilder, DefaultSitemapBuilder>();

            // index treeNode
            services.AddScoped<ISitemapNodeProviderFactory, SitemapNodeProviderFactory<SitemapIndexNode>>();
            services.AddSingleton<ISitemapNodeBuilder, SitemapIndexNodeBuilder>();
            services.AddScoped<IDisplayDriver<SitemapNode>, SitemapIndexNodeDriver>();
            //sitemap part
            services.AddScoped<IContentPartDisplayDriver, SitemapPartDisplay>();
            services.AddContentPart<SitemapPart>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                 name: SitemapRouteConstraint.RouteKey,
                 areaName: "OrchardCore.Sitemaps",
                 pattern: "{*sitemaps}",
                 constraints: new { sitemaps = new SitemapRouteConstraint() },
                 defaults: new { controller = "Sitemaps", action = "Index" }
             );

            var sitemapSetService = serviceProvider.GetService<ISitemapSetService>();
            var sitemapSets = sitemapSetService.GetAsync().GetAwaiter().GetResult();
            foreach (var sitemapSet in sitemapSets.Where(x => x.Enabled))
            {
                var rootPath = String.Empty; // sitemapSet.BasePath.ToString().TrimStart('/');
                sitemapSetService.BuildSitemapRouteEntries(sitemapSet.SitemapNodes, rootPath);
            }
        }
    }
}
