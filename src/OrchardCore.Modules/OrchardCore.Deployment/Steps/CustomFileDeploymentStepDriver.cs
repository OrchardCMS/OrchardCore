using System.Threading.Tasks;
using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Deployment.Steps
{
    public class CustomFileDeploymentStepDriver : DisplayDriver<DeploymentStep, CustomFileDeploymentStep>
    {
        public override IDisplayResult Display(CustomFileDeploymentStep step)
        {
            return
                Combine(
                    View("CustomFileDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("CustomFileDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(CustomFileDeploymentStep step)
        {
            return Initialize<CustomFileDeploymentStepViewModel>("CustomFileDeploymentStep_Fields_Edit", model =>
            {
                model.FileContent = step.FileContent;
                model.FileName = step.FileName;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(CustomFileDeploymentStep step, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(step, Prefix, x => x.FileName, x => x.FileContent);

            return Edit(step);
        }
    }
}
