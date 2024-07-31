using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Templates.ViewModels;

namespace OrchardCore.Templates.Deployment
{
    public class AllTemplatesDeploymentStepDriver : DisplayDriver<DeploymentStep, AllTemplatesDeploymentStep>
    {
        public override Task<IDisplayResult> DisplayAsync(AllTemplatesDeploymentStep step, BuildDisplayContext context)
        {
            return
                CombineAsync(
                    View("AllTemplatesDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("AllTemplatesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override Task<IDisplayResult> EditAsync(AllTemplatesDeploymentStep step, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                Initialize<AllTemplatesDeploymentStepViewModel>("AllTemplatesDeploymentStep_Fields_Edit", model =>
                {
                    model.ExportAsFiles = step.ExportAsFiles;
                }).Location("Content")
            );
        }
        public override async Task<IDisplayResult> UpdateAsync(AllTemplatesDeploymentStep step, UpdateEditorContext context)
        {
            await context.Updater.TryUpdateModelAsync(step, Prefix, x => x.ExportAsFiles);

            return await EditAsync(step, context);
        }
    }
}
