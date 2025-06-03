using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Indexing.Core.Deployments;
using OrchardCore.Search.AzureAI.ViewModels;

namespace OrchardCore.Indexing.Deployments;

public sealed class RebuildIndexEntityDeploymentStepDriver
    : DisplayDriver<DeploymentStep, RebuildIndexEntityDeploymentStep>
{
    private readonly IIndexEntityStore _store;

    public RebuildIndexEntityDeploymentStepDriver(IIndexEntityStore store)
    {
        _store = store;
    }

    public override Task<IDisplayResult> DisplayAsync(RebuildIndexEntityDeploymentStep step, BuildDisplayContext context)
    {
        return CombineAsync(
            View("RebuildIndexEntityDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
            View("RebuildIndexEntityDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
        );
    }

    public override IDisplayResult Edit(RebuildIndexEntityDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<RebuildIndexEntityDeploymentStepViewModel>("RebuildIndexEntityDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.IndexNames = step.Indexes;
            model.AllIndexNames = (await _store.GetAllAsync()).Select(x => x.IndexName).ToArray();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(RebuildIndexEntityDeploymentStep step, UpdateEditorContext context)
    {
        var model = new RebuildIndexEntityDeploymentStepViewModel();

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
