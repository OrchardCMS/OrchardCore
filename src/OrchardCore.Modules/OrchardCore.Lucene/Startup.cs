using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Lucene.Endpoints.Management;
using OrchardCore.RemoteManagement;
using Lucene.Net.Analysis.Standard;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.Indexing;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Models;
using OrchardCore.Lucene.Core;
using OrchardCore.Lucene.Core.Handlers;
using OrchardCore.Lucene.Deployment;
using OrchardCore.Lucene.Drivers;
using OrchardCore.Lucene.Recipes;
using OrchardCore.Lucene.Services;
using OrchardCore.Lucene.Settings;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Queries;
using OrchardCore.Queries.Core;
using OrchardCore.Queries.Sql.Migrations;
using OrchardCore.Recipes;
using OrchardCore.Search;
using OrchardCore.Search.Lucene.DataMigrations;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Lucene;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataMigration<Migrations>();
        services.TryAddSingleton<ILuceneIndexStore, LuceneIndexStore>();
        services.TryAddSingleton<ILuceneIndexingState, LuceneIndexingState>();
        services.TryAddSingleton<LuceneAnalyzerManager>();
        services.TryAddScoped<ILuceneSearchQueryService, LuceneSearchQueryService>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddPermissionProvider<Permissions>();

        services.Configure<LuceneOptions>(o =>
            o.Analyzers.Add(new LuceneAnalyzer(LuceneConstants.DefaultAnalyzer,
                new StandardAnalyzer(LuceneConstants.DefaultVersion))));

        services.AddDisplayDriver<Query, LuceneQueryDisplayDriver>();

        services
            .AddLuceneQueries()
            .AddQuerySource<LuceneQuerySource>(LuceneQuerySource.SourceName);

        services.AddDataMigration<LuceneQueryMigrations>();
        services.AddScoped<IQueryHandler, LuceneQueryHandler>();

        services.AddDisplayDriver<IndexProfile, LuceneIndexProfileDisplayDriver>();

        services.AddIndexProfileHandler<LuceneIndexProfileHandler>();
        services.AddIndexProfileHandler<LuceneIndexValidationHandler>();
    }
}

[RequireFeatures("OrchardCore.Recipes.Core")]
public sealed class RecipeStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddRecipeExecutionStep<LuceneIndexStep>();
        services.AddRecipeExecutionStep<LuceneIndexRebuildStep>();
        services.AddRecipeExecutionStep<LuceneIndexResetStep>();
    }
}

[RequireFeatures("OrchardCore.Contents")]
public sealed class ContentsStartup : StartupBase
{
    internal readonly IStringLocalizer S;

    public ContentsStartup(IStringLocalizer<ContentsStartup> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IRemoteManagementCapabilityProvider, LuceneIndexCapabilityProvider>();
        services.Configure<IndexLifecycleOptions>(options => options.RemoteProviders.Add(LuceneConstants.ProviderName));
        services.AddDataMigration<IndexingMigrations>();

        // Register after IndexingMigrations so its deferred task, which rewrites obsolete per-index role
        // permissions to the new dynamic permissions, runs after the index profiles have been created.
        services.AddDataMigration<PermissionMigrations>();

        services
            .AddIndexProfileHandler<LuceneContentIndexProfileHandler>()
            .AddLuceneIndexingSource(IndexingConstants.ContentsIndexSource, o =>
            {
                o.DisplayName = S["Content in Lucene"];
                o.Description = S["Create an Lucene index based on site contents."];
            });
    }
    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        => routes.AddLuceneIndexDefinitionEndpoints();
}

[RequireFeatures("OrchardCore.Search")]
public sealed class SearchStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSearchService<LuceneSearchService>(LuceneConstants.ProviderName);
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<LuceneIndexDeploymentSource, LuceneIndexDeploymentStep, LuceneIndexDeploymentStepDriver>();
        services.AddScoped<IDeploymentStepDefinition>(provider => new NamedSelectionDeploymentStepDefinition<LuceneIndexDeploymentStep>(
            nameof(LuceneIndexDeploymentStep), "indexNames", async () => (await provider.GetRequiredService<IIndexProfileStore>().GetByProviderAsync(LuceneConstants.ProviderName)).Select(index => index.IndexName),
            step => (step.IncludeAll, step.IndexNames), (step, includeAll, names) =>
            {
                step.IncludeAll = includeAll;
                step.IndexNames = names;
            }));
        services.AddDeployment<LuceneIndexRebuildDeploymentSource, LuceneIndexRebuildDeploymentStep, LuceneIndexRebuildDeploymentStepDriver>();
        services.AddScoped<IDeploymentStepDefinition>(provider => new NamedSelectionDeploymentStepDefinition<LuceneIndexRebuildDeploymentStep>(
            nameof(LuceneIndexRebuildDeploymentStep), "indexNames", async () => (await provider.GetRequiredService<IIndexProfileStore>().GetByProviderAsync(LuceneConstants.ProviderName)).Select(index => index.IndexName),
            step => (step.IncludeAll, step.IndexNames), (step, includeAll, names) =>
            {
                step.IncludeAll = includeAll;
                step.IndexNames = names;
            }));
        services.AddDeployment<LuceneIndexResetDeploymentSource, LuceneIndexResetDeploymentStep, LuceneIndexResetDeploymentStepDriver>();
        services.AddScoped<IDeploymentStepDefinition>(provider => new NamedSelectionDeploymentStepDefinition<LuceneIndexResetDeploymentStep>(
            nameof(LuceneIndexResetDeploymentStep), "indexNames", async () => (await provider.GetRequiredService<IIndexProfileStore>().GetByProviderAsync(LuceneConstants.ProviderName)).Select(index => index.IndexName),
            step => (step.IncludeAll, step.IndexNames), (step, includeAll, names) =>
            {
                step.IncludeAll = includeAll;
                step.IndexNames = names;
            }));
    }
}

[Feature("OrchardCore.Search.Lucene.ContentPicker")]
public sealed class LuceneContentPickerStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IContentPickerResultProvider, LuceneContentPickerResultProvider>();
        services.AddScoped<IContentPartFieldDefinitionDisplayDriver, ContentPickerFieldLuceneEditorSettingsDriver>();
        services.AddShapeAttributes<LuceneContentPickerShapeProvider>();
    }
}

[RequireFeatures("OrchardCore.ContentTypes")]
public sealed class ContentTypesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IContentTypePartDefinitionDisplayDriver, ContentTypePartIndexSettingsDisplayDriver>();
        services.AddScoped<IContentPartFieldDefinitionDisplayDriver, ContentPartFieldIndexSettingsDisplayDriver>();
    }
}

[Feature("OrchardCore.Search.Lucene")]
public sealed class LegacyFeatureStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataMigration<LegacyFeatureMigrations>();
    }
}

internal sealed class LegacyFeatureMigrations : DataMigration
{
    public static int Create()
    {
        ShellScope.AddDeferredTask(async scope =>
        {
            var featuresManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();

            if (await featuresManager.IsFeatureEnabledAsync("OrchardCore.Lucene"))
            {
                return;
            }

            await featuresManager.EnableFeaturesAsync("OrchardCore.Lucene");
        });

        return 1;
    }
}
