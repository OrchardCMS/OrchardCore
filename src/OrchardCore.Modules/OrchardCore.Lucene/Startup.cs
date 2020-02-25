using System;
using Lucene.Net.Analysis.Standard;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Lucene.Controllers;
using OrchardCore.Lucene.Deployment;
using OrchardCore.Lucene.Drivers;
using OrchardCore.Lucene.Handlers;
using OrchardCore.Lucene.Model;
using OrchardCore.Lucene.Recipes;
using OrchardCore.Lucene.Services;
using OrchardCore.Lucene.Settings;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
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
        private readonly AdminOptions _adminOptions;

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<LuceneIndexingState>();
            services.AddSingleton<LuceneIndexSettingsService>();
            services.AddSingleton<LuceneIndexManager>();
            services.AddSingleton<LuceneAnalyzerManager>();
            services.AddScoped<LuceneIndexingService>();
            services.AddScoped<ISearchQueryService, SearchQueryService>();

            services.AddScoped<IContentTypePartDefinitionDisplayDriver, ContentTypePartIndexSettingsDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, ContentPartFieldIndexSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions>();

            services.Configure<LuceneOptions>(o =>
                o.Analyzers.Add(new LuceneAnalyzer(LuceneSettings.StandardAnalyzer,
                    new StandardAnalyzer(LuceneSettings.DefaultVersion))));

            services.AddScoped<IDisplayDriver<ISite>, LuceneSiteSettingsDisplayDriver>();
            services.AddScoped<IDisplayDriver<Query>, LuceneQueryDisplayDriver>();

            services.AddScoped<IContentHandler, LuceneIndexingContentHandler>();
            services.AddLuceneQueries();

            // LuceneQuerySource is registered for both the Queries module and local usage
            services.AddScoped<IQuerySource, LuceneQuerySource>();
            services.AddScoped<LuceneQuerySource>();
            services.AddRecipeExecutionStep<LuceneIndexStep>();

            services.AddScoped<IShapeTableProvider, SearchShapesTableProvider>();
            services.AddShapeAttributes<SearchShapes>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "Lucene.Search",
                areaName: "OrchardCore.Lucene",
                pattern: "Search",
                defaults: new { controller = "Search", action = "Search" }
            );

            var adminControllerName = typeof(AdminController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "Lucene.Index",
                areaName: "OrchardCore.Lucene",
                pattern: _adminOptions.AdminUrlPrefix + "/Lucene/Index",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Index) }
            );

            routes.MapAreaControllerRoute(
                name: "Lucene.Delete",
                areaName: "OrchardCore.Lucene",
                pattern: _adminOptions.AdminUrlPrefix + "/Lucene/Delete/{id}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Delete) }
            );

            routes.MapAreaControllerRoute(
                name: "Lucene.Query",
                areaName: "OrchardCore.Lucene",
                pattern: _adminOptions.AdminUrlPrefix + "/Lucene/Query",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Query) }
            );

            routes.MapAreaControllerRoute(
                name: "Lucene.Rebuild",
                areaName: "OrchardCore.Lucene",
                pattern: _adminOptions.AdminUrlPrefix + "/Lucene/Rebuild/{id}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Rebuild) }
            );

            routes.MapAreaControllerRoute(
                name: "Lucene.Reset",
                areaName: "OrchardCore.Lucene",
                pattern: _adminOptions.AdminUrlPrefix + "/Lucene/Reset/{id}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Reset) }
            );
        }
    }

    [RequireFeatures("OrchardCore.Deployment")]
    public class DeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDeploymentSource, LuceneIndexDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<LuceneIndexDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, LuceneIndexDeploymentStepDriver>();

            services.AddTransient<IDeploymentSource, LuceneSettingsDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<LuceneSettingsDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, LuceneSettingsDeploymentStepDriver>();
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

    [Feature("OrchardCore.Lucene.ContentPicker")]
    public class LuceneContentPickerStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentPickerResultProvider, LuceneContentPickerResultProvider>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, ContentPickerFieldLuceneEditorSettingsDriver>();
            services.AddShapeAttributes<LuceneContentPickerShapeProvider>();
        }
    }
}
