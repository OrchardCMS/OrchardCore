using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Templates.ViewModels;

namespace OrchardCore.Templates.Deployment
{
    public class AllTemplatesDeploymentStepDriver : DisplayDriver<DeploymentStep, AllTemplatesDeploymentStep>
    {
        public override IDisplayResult Display(AllTemplatesDeploymentStep step)
        {
            return
                Combine(
                    View("AllTemplatesDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("AllTemplatesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AllTemplatesDeploymentStep step)
        {
            return Initialize<AllTemplatesDeploymentStepViewModel>("AllTemplatesDeploymentStep_Fields_Edit", model => model.ExportAsFiles = step.ExportAsFiles).Location("Content");
        }
        public override async Task<IDisplayResult> UpdateAsync(AllTemplatesDeploymentStep step, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(step, Prefix, x => x.ExportAsFiles);

            return Edit(step);
        }
    }
}
