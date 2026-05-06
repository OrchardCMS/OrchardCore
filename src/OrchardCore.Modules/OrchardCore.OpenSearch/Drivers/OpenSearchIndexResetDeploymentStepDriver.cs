using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Indexing;
using OrchardCore.OpenSearch.Core.Deployment;
using OrchardCore.OpenSearch.ViewModels;

namespace OrchardCore.OpenSearch.Drivers;

public sealed class OpenSearchIndexResetDeploymentStepDriver : DisplayDriver<DeploymentStep, OpenSearchIndexResetDeploymentStep>
{
    private readonly IIndexProfileStore _store;

    public OpenSearchIndexResetDeploymentStepDriver(IIndexProfileStore store)
    {
        _store = store;
    }

    public override Task<IDisplayResult> DisplayAsync(OpenSearchIndexResetDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("OpenSearchIndexResetDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("OpenSearchIndexResetDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(OpenSearchIndexResetDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<OpenSearchIndexResetDeploymentStepViewModel>("OpenSearchIndexResetDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.IndexNames = step.Indices;
            model.AllIndexNames = (await _store.GetByProviderAsync(OpenSearchConstants.ProviderName)).Select(x => x.IndexName).ToArray();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(OpenSearchIndexResetDeploymentStep resetIndexStep, UpdateEditorContext context)
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
