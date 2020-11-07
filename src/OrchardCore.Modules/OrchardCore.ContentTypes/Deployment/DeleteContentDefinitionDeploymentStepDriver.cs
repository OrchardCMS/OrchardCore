using System;
using System.Threading.Tasks;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Deployment
{
    public class DeleteContentDefinitionDeploymentStepDriver : DisplayDriver<DeploymentStep, DeleteContentDefinitionDeploymentStep>
    {
        public override IDisplayResult Display(DeleteContentDefinitionDeploymentStep step)
        {
            return
                Combine(
                    View("DeleteContentDefinitionDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("DeleteContentDefinitionDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(DeleteContentDefinitionDeploymentStep step)
        {
            return Initialize<DeleteContentDefinitionStepViewModel>("DeleteContentDefinitionDeploymentStep_Fields_Edit", model =>
            {
                model.ContentParts = String.Join(", ", step.ContentParts);
                model.ContentTypes = String.Join(", ", step.ContentTypes);
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(DeleteContentDefinitionDeploymentStep step, IUpdateModel updater)
        {
            var model = new DeleteContentDefinitionStepViewModel();

            if (await updater.TryUpdateModelAsync(model, Prefix))
            {
                step.ContentTypes = model.ContentTypes.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                step.ContentParts = model.ContentParts.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            }

            return Edit(step);
        }
    }
}
