using Microsoft.AspNetCore.Mvc.Rendering;
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
            model.Indexes = (await _store.GetAllAsync()).Select(x => new SelectListItem(x.Name, x.Id)
            {
                Selected = step.IndexeIds?.Contains(x.IndexName) ?? false,
            }).OrderBy(x => x.Text)
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
            step.IndexeIds = [];
        }
        else
        {
            if (model.Indexes == null || model.Indexes.Length == 0)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.Indexes), S["At least one index is required."]);
            }

            step.IncludeAll = false;
            step.IndexeIds = model.Indexes.Where(x => x.Selected).Select(x => x.Value).ToArray();
        }

        return Edit(step, context);
    }
}
