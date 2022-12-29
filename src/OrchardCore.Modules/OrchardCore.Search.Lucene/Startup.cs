using System;
using Fluid;
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
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Queries;
using OrchardCore.Recipes;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Abstractions.ViewModels;
using OrchardCore.Search.Lucene.Controllers;
using OrchardCore.Search.Lucene.Deployment;
using OrchardCore.Search.Lucene.Drivers;
using OrchardCore.Search.Lucene.Handlers;
using OrchardCore.Search.Lucene.Model;
using OrchardCore.Search.Lucene.Recipes;
using OrchardCore.Search.Lucene.Services;
using OrchardCore.Search.Lucene.Settings;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Search.Lucene
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
            services.Configure<TemplateOptions>(o =>
            {
                o.MemberAccessStrategy.Register<SearchIndexViewModel>();
                o.MemberAccessStrategy.Register<SearchFormViewModel>();
                o.MemberAccessStrategy.Register<SearchResultsViewModel>();
            });

            services.AddScoped<IDataMigration, Migrations>();
            services.AddSingleton<SearchProvider, LuceneSearchProvider>();
            services.AddSingleton<LuceneIndexingState>();
            services.AddSingleton<LuceneIndexSettingsService>();
            services.AddSingleton<LuceneIndexManager>();
            services.AddSingleton<LuceneAnalyzerManager>();
            services.AddScoped<LuceneIndexingService>();
            services.AddScoped<IModularTenantEvents, LuceneIndexInitializerService>();
            services.AddScoped<ILuceneSearchQueryService, LuceneSearchQueryService>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions>();

            services.Configure<LuceneOptions>(o =>
                o.Analyzers.Add(new LuceneAnalyzer(LuceneSettings.StandardAnalyzer,
                    new StandardAnalyzer(LuceneSettings.DefaultVersion))));

            services.AddScoped<IDisplayDriver<ISite>, LuceneSettingsDisplayDriver>();
            services.AddScoped<IDisplayDriver<Query>, LuceneQueryDisplayDriver>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, ContentTypePartIndexSettingsDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, ContentPartFieldIndexSettingsDisplayDriver>();

            services.AddScoped<IContentHandler, LuceneIndexingContentHandler>();
            services.AddLuceneQueries();

            // LuceneQuerySource is registered for both the Queries module and local usage
            services.AddScoped<IQuerySource, LuceneQuerySource>();
            services.AddScoped<LuceneQuerySource>();
            services.AddRecipeExecutionStep<LuceneIndexStep>();
            services.AddRecipeExecutionStep<LuceneIndexRebuildStep>();
            services.AddRecipeExecutionStep<LuceneIndexResetStep>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var adminControllerName = typeof(AdminController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "Lucene.Index",
                areaName: "OrchardCore.Search.Lucene",
                pattern: _adminOptions.AdminUrlPrefix + "/Lucene/Index",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Index) }
            );

            routes.MapAreaControllerRoute(
                name: "Lucene.Delete",
                areaName: "OrchardCore.Search.Lucene",
                pattern: _adminOptions.AdminUrlPrefix + "/Lucene/Delete/{id}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Delete) }
            );

            routes.MapAreaControllerRoute(
                name: "Lucene.Query",
                areaName: "OrchardCore.Search.Lucene",
                pattern: _adminOptions.AdminUrlPrefix + "/Lucene/Query",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Query) }
            );

            routes.MapAreaControllerRoute(
                name: "Lucene.Rebuild",
                areaName: "OrchardCore.Search.Lucene",
                pattern: _adminOptions.AdminUrlPrefix + "/Lucene/Rebuild/{id}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Rebuild) }
            );

            routes.MapAreaControllerRoute(
                name: "Lucene.Reset",
                areaName: "OrchardCore.Search.Lucene",
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

            services.AddTransient<IDeploymentSource, LuceneIndexRebuildDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<LuceneIndexRebuildDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, LuceneIndexRebuildDeploymentStepDriver>();

            services.AddTransient<IDeploymentSource, LuceneIndexResetDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<LuceneIndexResetDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, LuceneIndexResetDeploymentStepDriver>();
        }
    }

    [Feature("OrchardCore.Search.Lucene.Worker")]
    public class LuceneWorkerStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IBackgroundTask, IndexingBackgroundTask>();
        }
    }

    [Feature("OrchardCore.Search.Lucene.ContentPicker")]
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
