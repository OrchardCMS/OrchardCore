using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Templates.ViewModels;

namespace OrchardCore.Templates.Deployment
{
    public class AllAdminTemplatesDeploymentStepDriver : DisplayDriver<DeploymentStep, AllAdminTemplatesDeploymentStep>
    {
        public override IDisplayResult Display(AllAdminTemplatesDeploymentStep step)
        {
            return
                Combine(
                    View("AllAdminTemplatesDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("AllAdminTemplatesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AllAdminTemplatesDeploymentStep step)
        {
            return Initialize<AllAdminTemplatesDeploymentStepViewModel>("AllAdminTemplatesDeploymentStep_Fields_Edit", model => model.ExportAsFiles = step.ExportAsFiles).Location("Content");
        }
        public override async Task<IDisplayResult> UpdateAsync(AllAdminTemplatesDeploymentStep step, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(step, Prefix, x => x.ExportAsFiles);

            return Edit(step);
        }
    }
}
