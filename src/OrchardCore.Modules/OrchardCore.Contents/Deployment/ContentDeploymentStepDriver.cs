using System;
using System.Threading.Tasks;
using OrchardCore.Contents.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Deployment
{
    public class ContentDeploymentStepDriver : DisplayDriver<DeploymentStep, ContentDeploymentStep>
    {
        public override IDisplayResult Display(ContentDeploymentStep step)
        {
            return
                Combine(
                    View("ContentDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("ContentDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(ContentDeploymentStep step)
        {
            return Initialize<ContentDeploymentStepViewModel>("ContentDeploymentStep_Fields_Edit", model =>
            {
                model.ContentTypes = step.ContentTypes;
                model.ExportAsSetupRecipe = step.ExportAsSetupRecipe;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentDeploymentStep step, IUpdateModel updater)
        {
            // Initializes the value to empty otherwise the model is not updated if no type is selected.
            step.ContentTypes = Array.Empty<string>();

            await updater.TryUpdateModelAsync(step, Prefix, x => x.ContentTypes, x => x.ExportAsSetupRecipe);

            return Edit(step);
        }
    }
}
