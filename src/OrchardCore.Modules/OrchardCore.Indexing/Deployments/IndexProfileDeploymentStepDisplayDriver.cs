using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Indexing.Core.Deployments;
using OrchardCore.Indexing.Deployments.ViewModels;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Indexing.Deployments;

internal sealed class IndexProfileDeploymentStepDisplayDriver : DisplayDriver<DeploymentStep, IndexProfileDeploymentStep>
{
    private readonly IIndexProfileStore _store;

    internal readonly IStringLocalizer S;

    public IndexProfileDeploymentStepDisplayDriver(
        IIndexProfileStore store,
        IStringLocalizer<IndexProfileDeploymentStepDisplayDriver> stringLocalizer)
    {
        _store = store;
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(IndexProfileDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("IndexProfileDeploymentStep_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("IndexProfileDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(IndexProfileDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<IndexProfileDeploymentStepViewModel>("IndexProfileDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.Indexes = (await _store.GetAllAsync()).Select(x => new SelectListItem(x.Name, x.Name)
            {
                Selected = step.IndexNames?.Contains(x.Name) ?? false,
            }).OrderBy(x => x.Text)
            .ToArray();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(IndexProfileDeploymentStep step, UpdateEditorContext context)
    {
        var model = new IndexProfileDeploymentStepViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (model.IncludeAll)
        {
            step.IncludeAll = true;
            step.IndexNames = [];
        }
        else
        {
            if (model.Indexes == null || model.Indexes.Length == 0)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.Indexes), S["At least one index is required."]);
            }

            step.IncludeAll = false;
            step.IndexNames = model.Indexes.Where(x => x.Selected).Select(x => x.Value).ToArray();
        }

        return Edit(step, context);
    }
}
