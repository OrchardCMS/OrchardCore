using System.Threading.Tasks;
using OrchardCore.Contents.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Deployment
{
    public class AllContentDeploymentStepDriver : DisplayDriver<DeploymentStep, AllContentDeploymentStep>
    {
        public override IDisplayResult Display(AllContentDeploymentStep step)
        {
            return
                Combine(
                    View("AllContentDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("AllContentDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AllContentDeploymentStep step)
        {
            return Initialize<AllContentDeploymentStepViewModel>("AllContentDeploymentStep_Fields_Edit", model =>
            {
                model.ExportAsSetupRecipe = step.ExportAsSetupRecipe;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(AllContentDeploymentStep step, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(step, Prefix, x => x.ExportAsSetupRecipe);

            return Edit(step);
        }
    }
}
