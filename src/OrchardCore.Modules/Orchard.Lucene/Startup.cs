using System;
using Lucene.Net.Analysis.Standard;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.BackgroundTasks;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentTypes.Editors;
using Orchard.DisplayManagement.Handlers;
using Orchard.Environment.Navigation;
using Orchard.Lucene.Drivers;
using Orchard.Lucene.Handlers;
using Orchard.Lucene.Recipes;
using Orchard.Lucene.Services;
using Orchard.Lucene.Settings;
using Orchard.Queries;
using Orchard.Recipes;
using Orchard.Security.Permissions;
using Orchard.Settings;

namespace Orchard.Lucene
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
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddSingleton<LuceneIndexManager>();
            services.AddSingleton<LuceneAnalyzerManager>();

            services.AddScoped<IDisplayDriver<ISite>, LuceneSiteSettingsDisplayDriver>();
            services.AddScoped<IDisplayDriver<Query>, LuceneQueryDisplayDriver>();

            services.AddScoped<IContentHandler, LuceneIndexingContentHandler>();
            services.AddLuceneQueries();

            // LuceneQuerySource is registered for both the Queries module and local usage
            services.AddScoped<IQuerySource, LuceneQuerySource>();
            services.AddScoped<LuceneQuerySource>();
            services.AddRecipeExecutionStep<LuceneIndexStep>();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "Lucene.Search",
                areaName: "Orchard.Lucene",
                template: "Search/{id?}",
                defaults: new { controller = "Search", action = "Index", id = "" }
            );

            routes.MapAreaRoute(
                name: "Api.Lucene.Content",
                areaName: "Orchard.Lucene",
                template: "api/lucene/content",
                defaults: new { controller = "Api", action = "Content" }
            );

            routes.MapAreaRoute(
                name: "Api.Lucene.Documents",
                areaName: "Orchard.Lucene",
                template: "api/lucene/documents",
                defaults: new { controller = "Api", action = "Documents" }
            );

            var luceneAnalyzerManager = serviceProvider.GetRequiredService<LuceneAnalyzerManager>();
            luceneAnalyzerManager.RegisterAnalyzer(new LuceneAnalyzer("StandardAnalyzer", new StandardAnalyzer(LuceneSettings.DefaultVersion)));
        }
    }

    [Feature("Orchard.Lucene.Worker")]
    public class LuceneWorkerStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IBackgroundTask, IndexingBackgroundTask>();
        }
    }
}
