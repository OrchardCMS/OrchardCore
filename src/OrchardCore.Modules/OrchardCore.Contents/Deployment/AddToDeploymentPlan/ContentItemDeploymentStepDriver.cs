using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Contents.Deployment.AddToDeploymentPlan
{
    public class ContentItemDeploymentStepDriver : DisplayDriver<DeploymentStep, ContentItemDeploymentStep>
    {
        private readonly IContentManager _contentManager;
        protected readonly IStringLocalizer S;

        public ContentItemDeploymentStepDriver(IContentManager contentManager,
            IStringLocalizer<ContentItemDeploymentStepDriver> stringLocalizer)
        {
            _contentManager = contentManager;
            S = stringLocalizer;
        }

        public override IDisplayResult Display(ContentItemDeploymentStep step)
        {
            return
                Combine(
                    View("ContentItemDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("ContentItemDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(ContentItemDeploymentStep step)
        {
            return Initialize<ContentItemDeploymentStepViewModel>("ContentItemDeploymentStep_Fields_Edit", model =>
            {
                model.ContentItemId = step.ContentItemId;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentItemDeploymentStep step, IUpdateModel updater)
        {
            var model = new ContentItemDeploymentStepViewModel();

            if (await updater.TryUpdateModelAsync(model, Prefix, x => x.ContentItemId))
            {
                var contentItem = await _contentManager.GetAsync(model.ContentItemId);
                if (contentItem == null)
                {
                    updater.ModelState.AddModelError(Prefix, nameof(step.ContentItemId), S["Your content item does not exist."]);
                }
                else
                {
                    step.ContentItemId = model.ContentItemId;
                }
            }

            return Edit(step);
        }
    }
}
