using System;
using Lucene.Net.Analysis.Standard;
using Microsoft.AspNetCore.Builder;
using OrchardCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Navigation;
using OrchardCore.Lucene.Drivers;
using OrchardCore.Lucene.Handlers;
using OrchardCore.Lucene.Recipes;
using OrchardCore.Lucene.Services;
using OrchardCore.Lucene.Settings;
using OrchardCore.Queries;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Lucene
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
                areaName: "OrchardCore.Lucene",
                template: "Search/{id?}",
                defaults: new { controller = "Search", action = "Index", id = "" }
            );

            routes.MapAreaRoute(
                name: "Api.Lucene.Content",
                areaName: "OrchardCore.Lucene",
                template: "api/lucene/content",
                defaults: new { controller = "Api", action = "Content" }
            );

            routes.MapAreaRoute(
                name: "Api.Lucene.Documents",
                areaName: "OrchardCore.Lucene",
                template: "api/lucene/documents",
                defaults: new { controller = "Api", action = "Documents" }
            );

            var luceneAnalyzerManager = serviceProvider.GetRequiredService<LuceneAnalyzerManager>();
            luceneAnalyzerManager.RegisterAnalyzer(new LuceneAnalyzer(LuceneSettings.StandardAnalyzer, new StandardAnalyzer(LuceneSettings.DefaultVersion)));
        }
    }

    [Feature("OrchardCore.Lucene.Worker")]
    public class LuceneWorkerStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IBackgroundTask, IndexingBackgroundTask>();
        }
    }
}
