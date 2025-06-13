using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Indexing.Core.Deployments;
using OrchardCore.Search.AzureAI.ViewModels;

namespace OrchardCore.Indexing.Deployments;

public sealed class RebuildIndexDeploymentStepDriver
    : DisplayDriver<DeploymentStep, RebuildIndexDeploymentStep>
{
    private readonly IIndexProfileStore _store;

    public RebuildIndexDeploymentStepDriver(IIndexProfileStore store)
    {
        _store = store;
    }

    public override Task<IDisplayResult> DisplayAsync(RebuildIndexDeploymentStep step, BuildDisplayContext context)
    {
        return CombineAsync(
            View("RebuildIndexDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
            View("RebuildIndexDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
        );
    }

    public override IDisplayResult Edit(RebuildIndexDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<RebuildIndexDeploymentStepViewModel>("RebuildIndexDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.IndexNames = step.IndexNames;
            model.AllIndexNames = (await _store.GetAllAsync()).Select(x => x.Name).ToArray();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(RebuildIndexDeploymentStep step, UpdateEditorContext context)
    {
        var model = new RebuildIndexDeploymentStepViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (model.IncludeAll)
        {
            // Clear index names if the user select include all.
            step.IncludeAll = true;
            step.IndexNames = [];
        }
        else
        {
            step.IncludeAll = false;
            step.IndexNames = model.IndexNames ?? [];
        }

        return Edit(step, context);
    }
}
