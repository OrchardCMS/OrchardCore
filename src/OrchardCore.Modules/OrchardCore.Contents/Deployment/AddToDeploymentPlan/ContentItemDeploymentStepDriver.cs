using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Contents.Deployment.AddToDeploymentPlan;

public sealed class ContentItemDeploymentStepDriver
    : DeploymentStepFieldsDriverBase<ContentItemDeploymentStep, ContentItemDeploymentStepViewModel>
{
    private readonly IContentManager _contentManager;

    internal readonly IStringLocalizer S;

    public ContentItemDeploymentStepDriver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _contentManager = serviceProvider.GetService<IContentManager>();
        S = serviceProvider.GetService<IStringLocalizer<ContentItemDeploymentStepDriver>>();
    }

    public override IDisplayResult Edit(ContentItemDeploymentStep step, Action<ContentItemDeploymentStepViewModel> intializeAction)
    {
        return base.Edit(step, model =>
        {
            model.ContentItemId = step.ContentItemId;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentItemDeploymentStep step, UpdateEditorContext context)
    {
        var model = new ContentItemDeploymentStepViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix, x => x.ContentItemId);
        var contentItem = await _contentManager.GetAsync(model.ContentItemId);

        if (contentItem == null)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(step.ContentItemId), S["Your content item does not exist."]);
        }
        else
        {
            step.ContentItemId = model.ContentItemId;
        }

        return Edit(step, context);
    }
}
