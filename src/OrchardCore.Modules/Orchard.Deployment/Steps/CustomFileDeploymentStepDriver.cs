using System.Threading.Tasks;
using Orchard.Deployment.ViewModels;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.Deployment.Steps
{
    public class CustomFileDeploymentStepDriver : DisplayDriver<DeploymentStep, CustomFileDeploymentStep>
    {
        public override IDisplayResult Display(CustomFileDeploymentStep step)
        {
            return 
                Combine(
                    Shape("CustomFileDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    Shape("CustomFileDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(CustomFileDeploymentStep step)
        {
            return Shape<CustomFileDeploymentStepViewModel>("CustomFileDeploymentStep_Fields_Edit", model =>
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
