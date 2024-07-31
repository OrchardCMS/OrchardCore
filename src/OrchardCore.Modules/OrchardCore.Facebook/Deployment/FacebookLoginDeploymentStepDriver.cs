using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Facebook.Deployment
{
    public class FacebookLoginDeploymentStepDriver : DisplayDriver<DeploymentStep, FacebookLoginDeploymentStep>
    {
        public override Task<IDisplayResult> DisplayAsync(FacebookLoginDeploymentStep step, BuildDisplayContext context)
        {
            return
                CombineAsync(
                    View("FacebookLoginDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("FacebookLoginDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override Task<IDisplayResult> EditAsync(FacebookLoginDeploymentStep step, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                View("FacebookLoginDeploymentStep_Edit", step).Location("Content")
            );
        }
    }
}
