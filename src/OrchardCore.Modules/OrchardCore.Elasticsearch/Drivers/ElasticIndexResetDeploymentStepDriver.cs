using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Indexing;
using OrchardCore.Elasticsearch.ViewModels;

namespace OrchardCore.Elasticsearch.Core.Deployment;

public sealed class ElasticIndexResetDeploymentStepDriver : DisplayDriver<DeploymentStep, ElasticsearchIndexResetDeploymentStep>
{
    private readonly IIndexProfileStore _store;

    internal readonly IStringLocalizer S;

    public ElasticIndexResetDeploymentStepDriver(IIndexProfileStore store, IStringLocalizer<ElasticIndexResetDeploymentStepDriver> stringLocalizer)
    {
        _store = store;
        S = stringLocalizer;
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
            model.AllIndexNames = (await _store.GetByProviderAsync(ElasticsearchConstants.ProviderName)).Select(x => x.IndexName).ToArray();
        }).Location(l => l.Zone("Content").Card(S["Indexes"], "10"));
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
