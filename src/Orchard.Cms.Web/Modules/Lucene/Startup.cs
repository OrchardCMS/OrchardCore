using System;
using Lucene.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.BackgroundTasks;
using Orchard.ContentTypes.Editors;
using Orchard.Environment.Navigation;

namespace Lucene
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<LuceneIndexingState>();
            services.AddScoped<LuceneIndexingService>();

            services.AddScoped<IContentTypePartDefinitionDisplayDriver, ContentTypePartIndexSettingsDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, ContentPartFieldIndexSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<LuceneIndexProvider>();
            services.AddScoped<LuceneSettings>();

            services.AddSingleton<IBackgroundTask, IndexingBackgroundTask>();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "Lucene.Search",
                areaName: "Lucene",
                template: "Search/{id?}",
                defaults: new { controller = "Search", action = "Index", id = "" }
            );

        }
    }
}
