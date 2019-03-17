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
            //new


            services.AddIdGeneration();
            services.AddScoped<ISitemapIdGenerator, SitemapIdGenerator>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<ISitemapSetService, SitemapSetService>();
            services.AddScoped<IDisplayManager<SitemapNode>, DisplayManager<SitemapNode>>();


            // index treeNode
            services.AddScoped<ISitemapNodeProviderFactory, SitemapNodeProviderFactory<SitemapIndexNode>>();
            //services.AddScoped<IAdminNodeNavigationBuilder, PlaceholderAdminNodeNavigationBuilder>();
            services.AddScoped<IDisplayDriver<SitemapNode>, SitemapIndexNodeDriver>();

            //old
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IDisplayDriver<ISite>, SitemapsSettingsDisplayDriver>();
            services.AddScoped<IPermissionProvider, Permissions>();

            services.AddScoped<IDataMigration, Migrations>();

            services.AddScoped<IContentPartDisplayDriver, SitemapPartDisplay>();
            services.AddSingleton<ContentPart, SitemapPart>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, SitemapPartSettingsDisplayDriver>();

            services.AddScoped<ISitemapManager, DefaultSitemapManager>();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "SitemapIndex",
                areaName: "OrchardCore.Sitemaps",
                template: "sitemap{number}.xml",
                defaults: new { controller = "Sitemaps", action = "Index" }
            );
            routes.MapAreaRoute(
                   name: "sitemap.xml",
                   areaName: "OrchardCore.Sitemaps",
                   template: "sitemap.xml",
                   defaults: new { controller = "Sitemaps", action = "Index" }
               );

        }
    }
}
