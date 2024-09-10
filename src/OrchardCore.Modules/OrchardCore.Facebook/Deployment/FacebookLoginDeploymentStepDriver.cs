using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Facebook.Deployment;

public sealed class FacebookLoginDeploymentStepDriver : DisplayDriver<DeploymentStep, FacebookLoginDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(FacebookLoginDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("FacebookLoginDeploymentStep_Summary", step).Location("Summary", "Content"),
                View("FacebookLoginDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(FacebookLoginDeploymentStep step, BuildEditorContext context)
    {
        return View("FacebookLoginDeploymentStep_Edit", step).Location("Content");
    }
}
