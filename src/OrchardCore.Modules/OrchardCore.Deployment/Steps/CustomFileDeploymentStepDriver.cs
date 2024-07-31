using System.Threading.Tasks;
using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Deployment.Steps
{
    public class CustomFileDeploymentStepDriver : DisplayDriver<DeploymentStep, CustomFileDeploymentStep>
    {
        public override Task<IDisplayResult> DisplayAsync(CustomFileDeploymentStep step, BuildDisplayContext context)
        {
            return
                CombineAsync(
                    View("CustomFileDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("CustomFileDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override Task<IDisplayResult> EditAsync(CustomFileDeploymentStep step, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                Initialize<CustomFileDeploymentStepViewModel>("CustomFileDeploymentStep_Fields_Edit", model =>
                {
                    model.FileContent = step.FileContent;
                    model.FileName = step.FileName;
                }).Location("Content")
            );
        }

        public override async Task<IDisplayResult> UpdateAsync(CustomFileDeploymentStep step, UpdateEditorContext context)
        {
            await context.Updater.TryUpdateModelAsync(step, Prefix, x => x.FileName, x => x.FileContent);

            return await EditAsync(step, context);
        }
    }
}
