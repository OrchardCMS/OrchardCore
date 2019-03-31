using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Sitemaps.Builders;
using OrchardCore.Sitemaps.Drivers;
using OrchardCore.Sitemaps.Models;
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
            services.AddScoped<ISitemapIdGenerator, SitemapIdGenerator>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddSingleton<ISitemapSetService, SitemapSetService>();
            services.AddScoped<IDisplayManager<SitemapNode>, DisplayManager<SitemapNode>>();
            services.AddScoped<ISitemapBuilder, SitemapBuilder>();

            // index treeNode
            services.AddScoped<ISitemapNodeProviderFactory, SitemapNodeProviderFactory<SitemapIndexNode>>();
            services.AddScoped<ISitemapNodeBuilder, SitemapIndexNodeBuilder>();
            services.AddScoped<IDisplayDriver<SitemapNode>, SitemapIndexNodeDriver>();
            //sitemap part
            services.AddScoped<IContentPartDisplayDriver, SitemapPartDisplay>();
            services.AddSingleton<ContentPart, SitemapPart>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, SitemapPartSettingsDisplayDriver>();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                  name: SitemapRouteConstraint.RouteKey,
                  areaName: "OrchardCore.Sitemaps",
                  template: "{*sitemaps}",
                  constraints: new { sitemaps = new SitemapRouteConstraint() },
                  defaults: new { controller = "Sitemaps", action = "Index" }
              );
        }
    }
}
