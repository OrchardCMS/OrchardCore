using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Contents.Deployment.AddToDeploymentPlan;

public sealed class ContentItemDeploymentStepDriver : DisplayDriver<DeploymentStep, ContentItemDeploymentStep>
{
    private readonly IContentManager _contentManager;

    internal readonly IStringLocalizer S;

    public ContentItemDeploymentStepDriver(IContentManager contentManager,
        IStringLocalizer<ContentItemDeploymentStepDriver> stringLocalizer)
    {
        _contentManager = contentManager;
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(ContentItemDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("ContentItemDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("ContentItemDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(ContentItemDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<ContentItemDeploymentStepViewModel>("ContentItemDeploymentStep_Fields_Edit", model =>
        {
            model.ContentItemId = step.ContentItemId;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentItemDeploymentStep step, UpdateEditorContext context)
    {
        var model = new ContentItemDeploymentStepViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix, x => x.ContentItemId);
        if (!await ContentItemDeploymentStepEditor.TryApplyAsync(_contentManager, step, model.ContentItemId))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(step.ContentItemId), S["Your content item does not exist."]);
        }

        return Edit(step, context);
    }
}
