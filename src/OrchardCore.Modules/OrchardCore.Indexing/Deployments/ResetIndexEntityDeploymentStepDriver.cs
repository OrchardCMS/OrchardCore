using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Indexing.Core.Deployments;
using OrchardCore.Search.AzureAI.ViewModels;

namespace OrchardCore.Indexing.Deployments;

public sealed class ResetIndexEntityDeploymentStepDriver
    : DisplayDriver<DeploymentStep, ResetIndexEntityDeploymentStep>
{
    private readonly IIndexEntityStore _store;

    public ResetIndexEntityDeploymentStepDriver(IIndexEntityStore store)
    {
        _store = store;
    }

    public override Task<IDisplayResult> DisplayAsync(ResetIndexEntityDeploymentStep step, BuildDisplayContext context)
    {
        return CombineAsync(
            View("ResetIndexEntityDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
            View("ResetIndexEntityDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
        );
    }

    public override IDisplayResult Edit(ResetIndexEntityDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<ResetIndexEntityDeploymentStepViewModel>("ResetIndexEntityDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.IndexNames = step.Indexes;
            model.AllIndexNames = (await _store.GetAllAsync()).Select(x => x.IndexName).ToArray();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ResetIndexEntityDeploymentStep step, UpdateEditorContext context)
    {
        step.Indexes = [];

        var model = new ResetIndexEntityDeploymentStepViewModel();

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
            step.IncludeAll = false;
            step.Indexes = model.IndexNames;
        }

        return Edit(step, context);
    }
}
