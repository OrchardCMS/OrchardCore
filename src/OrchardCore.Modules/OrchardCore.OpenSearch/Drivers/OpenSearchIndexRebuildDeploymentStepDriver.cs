using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Indexing;
using OrchardCore.OpenSearch.Core.Deployment;
using OrchardCore.OpenSearch.ViewModels;

namespace OrchardCore.OpenSearch.Drivers;

public sealed class OpenSearchIndexRebuildDeploymentStepDriver : DisplayDriver<DeploymentStep, OpenSearchIndexRebuildDeploymentStep>
{
    private readonly IIndexProfileStore _indexStore;

    public OpenSearchIndexRebuildDeploymentStepDriver(IIndexProfileStore indexStore)
    {
        _indexStore = indexStore;
    }

    public override Task<IDisplayResult> DisplayAsync(OpenSearchIndexRebuildDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("OpenSearchIndexRebuildDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("OpenSearchIndexRebuildDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(OpenSearchIndexRebuildDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<OpenSearchIndexRebuildDeploymentStepViewModel>("OpenSearchIndexRebuildDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.IndexNames = step.Indices;
            model.AllIndexNames = (await _indexStore.GetByProviderAsync(OpenSearchConstants.ProviderName)).Select(x => x.IndexName).ToArray();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(OpenSearchIndexRebuildDeploymentStep rebuildIndexStep, UpdateEditorContext context)
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
