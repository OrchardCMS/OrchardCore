using System.Threading.Tasks;
using OrchardCore.Contents.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Deployment
{
    public class AllContentDeploymentStepDriver : DisplayDriver<DeploymentStep, AllContentDeploymentStep>
    {
        public override Task<IDisplayResult> DisplayAsync(AllContentDeploymentStep step, BuildDisplayContext context)
        {
            return
                CombineAsync(
                    View("AllContentDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("AllContentDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override Task<IDisplayResult> EditAsync(AllContentDeploymentStep step, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                Initialize<AllContentDeploymentStepViewModel>("AllContentDeploymentStep_Fields_Edit", model =>
                {
                    model.ExportAsSetupRecipe = step.ExportAsSetupRecipe;
                }).Location("Content")
            );
        }

        public override async Task<IDisplayResult> UpdateAsync(AllContentDeploymentStep step, UpdateEditorContext context)
        {
            await context.Updater.TryUpdateModelAsync(step, Prefix, x => x.ExportAsSetupRecipe);

            return await EditAsync(step, context);
        }
    }
}
