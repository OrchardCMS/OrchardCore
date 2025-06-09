using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Indexing.Core.Deployments;
using OrchardCore.Search.AzureAI.ViewModels;

namespace OrchardCore.Indexing.Deployments;

public sealed class ResetIndexProfileDeploymentStepDriver
    : DisplayDriver<DeploymentStep, ResetIndexProfileDeploymentStep>
{
    private readonly IIndexProfileStore _store;

    public ResetIndexProfileDeploymentStepDriver(IIndexProfileStore store)
    {
        _store = store;
    }

    public override Task<IDisplayResult> DisplayAsync(ResetIndexProfileDeploymentStep step, BuildDisplayContext context)
    {
        return CombineAsync(
            View("ResetIndexProfileDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
            View("ResetIndexProfileDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
        );
    }

    public override IDisplayResult Edit(ResetIndexProfileDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<ResetIndexProfileDeploymentStepViewModel>("ResetIndexProfileDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.IndexNames = step.Indexes;
            model.AllIndexNames = (await _store.GetAllAsync()).Select(x => x.IndexName).ToArray();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ResetIndexProfileDeploymentStep step, UpdateEditorContext context)
    {
        step.Indexes = [];

        var model = new ResetIndexProfileDeploymentStepViewModel();

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
