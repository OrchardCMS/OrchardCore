using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Indexing;
using OrchardCore.Search.Elasticsearch.ViewModels;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment;

public sealed class ElasticIndexResetDeploymentStepDriver : DisplayDriver<DeploymentStep, ElasticsearchIndexResetDeploymentStep>
{
    private readonly IIndexEntityStore _indexStore;

    public ElasticIndexResetDeploymentStepDriver(IIndexEntityStore indexStore)
    {
        _indexStore = indexStore;
    }

    public override Task<IDisplayResult> DisplayAsync(ElasticsearchIndexResetDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("ElasticIndexResetDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("ElasticIndexResetDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(ElasticsearchIndexResetDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<ElasticIndexResetDeploymentStepViewModel>("ElasticIndexResetDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.IndexNames = step.Indices;
            model.AllIndexNames = (await _indexStore.GetAsync(ElasticsearchConstants.ProviderName)).Select(x => x.IndexName).ToArray();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ElasticsearchIndexResetDeploymentStep resetIndexStep, UpdateEditorContext context)
    {
        resetIndexStep.Indices = [];

        await context.Updater.TryUpdateModelAsync(resetIndexStep, Prefix, step => step.Indices, step => step.IncludeAll);

        if (resetIndexStep.IncludeAll)
        {
            resetIndexStep.Indices = [];
        }

        return Edit(resetIndexStep, context);
    }
}
