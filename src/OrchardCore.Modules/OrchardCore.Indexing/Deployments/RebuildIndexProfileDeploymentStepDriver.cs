using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Indexing.Core.Deployments;
using OrchardCore.Search.AzureAI.ViewModels;

namespace OrchardCore.Indexing.Deployments;

public sealed class RebuildIndexProfileDeploymentStepDriver
    : DisplayDriver<DeploymentStep, RebuildIndexProfileDeploymentStep>
{
    private readonly IIndexProfileStore _store;

    public RebuildIndexProfileDeploymentStepDriver(IIndexProfileStore store)
    {
        _store = store;
    }

    public override Task<IDisplayResult> DisplayAsync(RebuildIndexProfileDeploymentStep step, BuildDisplayContext context)
    {
        return CombineAsync(
            View("RebuildIndexProfileDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
            View("RebuildIndexProfileDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
        );
    }

    public override IDisplayResult Edit(RebuildIndexProfileDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<RebuildIndexProfileDeploymentStepViewModel>("RebuildIndexProfileDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.IndexNames = step.Indexes;
            model.AllIndexNames = (await _store.GetAllAsync()).Select(x => x.IndexName).ToArray();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(RebuildIndexProfileDeploymentStep step, UpdateEditorContext context)
    {
        var model = new RebuildIndexProfileDeploymentStepViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix,
            p => p.IncludeAll,
            p => p.IndexNames);

        if (step.IncludeAll)
        {
            // Clear index names if the user select include all.
            step.IncludeAll = true;
            step.Indexes = [];
        }
        else
        {
            step.IncludeAll = true;
            step.Indexes = model.IndexNames ?? [];
        }

        return Edit(step, context);
    }
}
