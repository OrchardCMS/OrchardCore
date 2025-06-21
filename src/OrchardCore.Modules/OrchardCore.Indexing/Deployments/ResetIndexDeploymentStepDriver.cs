using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Indexing.Core.Deployments;
using OrchardCore.Search.AzureAI.ViewModels;

namespace OrchardCore.Indexing.Deployments;

public sealed class ResetIndexDeploymentStepDriver
    : DisplayDriver<DeploymentStep, ResetIndexDeploymentStep>
{
    private readonly IIndexProfileStore _store;

    public ResetIndexDeploymentStepDriver(IIndexProfileStore store)
    {
        _store = store;
    }

    public override Task<IDisplayResult> DisplayAsync(ResetIndexDeploymentStep step, BuildDisplayContext context)
    {
        return CombineAsync(
            View("ResetIndexDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
            View("ResetIndexDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
        );
    }

    public override IDisplayResult Edit(ResetIndexDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<ResetIndexDeploymentStepViewModel>("ResetIndexDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.IndexNames = step.IndexNames;
            model.AllIndexNames = (await _store.GetAllAsync().ConfigureAwait(false)).Select(x => x.Name).ToArray();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ResetIndexDeploymentStep step, UpdateEditorContext context)
    {
        step.IndexNames = [];

        var model = new ResetIndexDeploymentStepViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix).ConfigureAwait(false);

        if (model.IncludeAll)
        {
            // Clear index names if the user select include all.
            step.IncludeAll = true;
            step.IndexNames = [];
        }
        else
        {
            step.IncludeAll = false;
            step.IndexNames = model.IndexNames;
        }

        return Edit(step, context);
    }
}
