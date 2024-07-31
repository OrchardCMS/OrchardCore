using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.OpenId.Deployment
{
    public class OpenIdValidationDeploymentStepDriver : DisplayDriver<DeploymentStep, OpenIdValidationDeploymentStep>
    {
        public override Task<IDisplayResult> DisplayAsync(OpenIdValidationDeploymentStep step, BuildDisplayContext context)
        {
            return
                CombineAsync(
                    View("OpenIdValidationDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("OpenIdValidationDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override Task<IDisplayResult> EditAsync(OpenIdValidationDeploymentStep step, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                View("OpenIdValidationDeploymentStep_Edit", step).Location("Content")
            );
        }
    }
}
