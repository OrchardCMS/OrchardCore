using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Deployment
{
    public class ContentDefinitionDeploymentStepDriver : DisplayDriver<DeploymentStep, ContentDefinitionDeploymentStep>
    {
        public override IDisplayResult Display(ContentDefinitionDeploymentStep step)
        {
            return
                Combine(
                    View("ContentDefinitionDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("ContentDefinitionDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(ContentDefinitionDeploymentStep step)
        {
            return Initialize<ContentDefinitionStepViewModel>("ContentDefinitionDeploymentStep_Fields_Edit", model =>
            {
                model.ContentParts = step.ContentParts;
                model.ContentTypes = step.ContentTypes;
                model.IncludeAll = step.IncludeAll;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentDefinitionDeploymentStep step, IUpdateModel updater)
        {
            // Initializes the value to empty otherwise the model is not updated if no type is selected.
            step.ContentTypes = Array.Empty<string>();
            step.ContentParts = Array.Empty<string>();

            await updater.TryUpdateModelAsync(
                step,
                Prefix,
                x => x.ContentTypes,
                x => x.ContentParts,
                x => x.IncludeAll);

            // don't have the selected option if include all
            if (step.IncludeAll)
            {
                step.ContentTypes = Array.Empty<string>();
                step.ContentParts = Array.Empty<string>();
            }
            else
            {
                step.ContentParts = step.ContentParts.Distinct().ToArray();
            }

            return Edit(step);
        }
    }
}
