using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Indexing.Core.Deployments;
using OrchardCore.Indexing.Deployments.ViewModels;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Indexing.Deployments;

internal sealed class IndexEntityDeploymentStepDisplayDriver : DisplayDriver<DeploymentStep, IndexEntityDeploymentStep>
{
    private readonly IIndexEntityStore _store;

    internal readonly IStringLocalizer S;

    public IndexEntityDeploymentStepDisplayDriver(
        IIndexEntityStore store,
        IStringLocalizer<IndexEntityDeploymentStepDisplayDriver> stringLocalizer)
    {
        _store = store;
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(IndexEntityDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("IndexEntityDeploymentStep_Summary", step).Location("Summary", "Content"),
                View("IndexEntityDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(IndexEntityDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<IndexEntityDeploymentStepViewModel>("IndexEntityDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.Indexes = (await _store.GetAllAsync()).Select(x => new IndexEntitySourceViewModel
            {
                IndexName = x.IndexName,
                DisplayText = x.DisplayText,
                IsSelected = step.Indexes?.Contains(x.IndexName) ?? false,
            }).OrderBy(x => x.DisplayText)
            .ToArray();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(IndexEntityDeploymentStep step, UpdateEditorContext context)
    {
        var model = new IndexEntityDeploymentStepViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix,
            p => p.IncludeAll,
            p => p.Indexes);

        if (model.IncludeAll)
        {
            step.IncludeAll = true;
            step.Indexes = [];
        }
        else
        {
            if (model.Indexes == null || model.Indexes.Length == 0)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.Indexes), S["At least one data-source is required."]);
            }

            step.IncludeAll = false;
            step.Indexes = model.Indexes.Where(x => x.IsSelected).Select(x => x.IndexName).ToArray();
        }

        return Edit(step, context);
    }
}
