using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Sitemaps.Deployment
{
    public class AllSitemapsDeploymentStepDriver : DisplayDriver<DeploymentStep, AllSitemapsDeploymentStep>
    {
        public override Task<IDisplayResult> DisplayAsync(AllSitemapsDeploymentStep step, BuildDisplayContext context)
        {
            return
                CombineAsync(
                    View("AllSitemapsDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("AllSitemapsDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override Task<IDisplayResult> EditAsync(AllSitemapsDeploymentStep step, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                View("AllSitemapsDeploymentStep_Edit", step).Location("Content")
            );
        }
    }
}
