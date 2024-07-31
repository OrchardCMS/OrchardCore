using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Media.Deployment
{
    public class AllMediaProfilesDeploymentStepDriver : DisplayDriver<DeploymentStep, AllMediaProfilesDeploymentStep>
    {
        public override Task<IDisplayResult> DisplayAsync(AllMediaProfilesDeploymentStep step, BuildDisplayContext context)
        {
            return
                CombineAsync(
                    View("AllMediaProfilesDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("AllMediaProfilesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override Task<IDisplayResult> EditAsync(AllMediaProfilesDeploymentStep step, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                View("AllMediaProfilesDeploymentStep_Edit", step).Location("Content")
            );
        }
    }
}
