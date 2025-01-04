using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Search.Elasticsearch.ViewModels;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment;

public sealed class ElasticIndexDeploymentStepDriver : DisplayDriver<DeploymentStep, ElasticsearchIndexDeploymentStep>
{
    private readonly ElasticsearchIndexSettingsService _elasticIndexSettingsService;

    public ElasticIndexDeploymentStepDriver(ElasticsearchIndexSettingsService elasticIndexSettingsService)
    {
        _elasticIndexSettingsService = elasticIndexSettingsService;
    }

    public override Task<IDisplayResult> DisplayAsync(ElasticsearchIndexDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("ElasticIndexDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                View("ElasticIndexDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(ElasticsearchIndexDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<ElasticIndexDeploymentStepViewModel>("ElasticIndexDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.IndexNames = step.IndexNames;
            model.AllIndexNames = (await _elasticIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ElasticsearchIndexDeploymentStep step, UpdateEditorContext context)
    {
        step.IndexNames = [];

        await context.Updater.TryUpdateModelAsync(step,
                                          Prefix,
                                          x => x.IndexNames,
                                          x => x.IncludeAll);

        // Don't have the selected option if include all.
        if (step.IncludeAll)
        {
            step.IndexNames = [];
        }

        return Edit(step, context);
    }
}
