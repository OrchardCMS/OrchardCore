using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Search.Elasticsearch.ViewModels;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment;

public sealed class ElasticIndexRebuildDeploymentStepDriver : DisplayDriver<DeploymentStep, ElasticsearchIndexRebuildDeploymentStep>
{
    private readonly ElasticsearchIndexSettingsService _elasticIndexSettingsService;

    public ElasticIndexRebuildDeploymentStepDriver(ElasticsearchIndexSettingsService elasticIndexSettingsService)
    {
        _elasticIndexSettingsService = elasticIndexSettingsService;
    }

    public override Task<IDisplayResult> DisplayAsync(ElasticsearchIndexRebuildDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("ElasticIndexRebuildDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                View("ElasticIndexRebuildDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(ElasticsearchIndexRebuildDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<ElasticIndexRebuildDeploymentStepViewModel>("ElasticIndexRebuildDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.IndexNames = step.Indices;
            model.AllIndexNames = (await _elasticIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ElasticsearchIndexRebuildDeploymentStep rebuildIndexStep, UpdateEditorContext context)
    {
        rebuildIndexStep.Indices = [];

        await context.Updater.TryUpdateModelAsync(rebuildIndexStep, Prefix, step => step.Indices, step => step.IncludeAll);

        if (rebuildIndexStep.IncludeAll)
        {
            rebuildIndexStep.Indices = [];
        }

        return Edit(rebuildIndexStep, context);
    }
}
