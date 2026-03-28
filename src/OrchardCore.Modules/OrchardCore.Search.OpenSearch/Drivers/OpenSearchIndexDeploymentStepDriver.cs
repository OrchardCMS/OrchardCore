using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Indexing;
using OrchardCore.Search.OpenSearch.Core.Deployment;
using OrchardCore.Search.OpenSearch.ViewModels;

namespace OrchardCore.Search.OpenSearch.Drivers;

public sealed class OpenSearchIndexDeploymentStepDriver : DisplayDriver<DeploymentStep, OpenSearchIndexDeploymentStep>
{
    private readonly IIndexProfileStore _store;

    public OpenSearchIndexDeploymentStepDriver(IIndexProfileStore store)
    {
        _store = store;
    }

    public override Task<IDisplayResult> DisplayAsync(OpenSearchIndexDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("OpenSearchIndexDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("OpenSearchIndexDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(OpenSearchIndexDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<OpenSearchIndexDeploymentStepViewModel>("OpenSearchIndexDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.IndexNames = step.IndexNames;
            model.AllIndexNames = (await _store.GetByProviderAsync(OpenSearchConstants.ProviderName)).Select(x => x.IndexName).ToArray();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(OpenSearchIndexDeploymentStep step, UpdateEditorContext context)
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
