using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Deployment.AddToDeploymentPlan
{
    public class ContentItemDeploymentStepDriver : DisplayDriver<DeploymentStep, ContentItemDeploymentStep>
    {
        private readonly IStringLocalizer S;

        public ContentItemDeploymentStepDriver(IStringLocalizer<ContentItemDeploymentStepDriver> stringLocalizer)
        {
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
                model.ContentItem = step.ContentItem?.ToString();
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentItemDeploymentStep step, IUpdateModel updater)
        {
            var model = new ContentItemDeploymentStepViewModel();

            await updater.TryUpdateModelAsync(model, Prefix, x => x.ContentItem);

            try
            {
                // Parse this to use the content item json convertor to validate the supplied json format.
                var jItem = JObject.Parse(model.ContentItem);
                jItem.Remove(nameof(ContentItem.Id));

                var contentItem = jItem.ToObject<ContentItem>();
                if (String.IsNullOrEmpty(contentItem.ContentItemId))
                {
                    updater.ModelState.AddModelError(nameof(step.ContentItem), S["You must supply a content item id."]);
                }

                if (String.IsNullOrEmpty(contentItem.ContentItemVersionId))
                {
                    updater.ModelState.AddModelError(nameof(step.ContentItem), S["You must supply a content item version id."]);
                }

                if (String.IsNullOrEmpty(contentItem.ContentType))
                {
                    updater.ModelState.AddModelError(nameof(step.ContentItem), S["You must supply a content type."]);
                }

                step.ContentItem = jItem;
            }
            catch
            {
                updater.ModelState.AddModelError(nameof(step.ContentItem), S["Your content item contains invalid JSON."]);
            }

            return Edit(step);
        }
    }
}
