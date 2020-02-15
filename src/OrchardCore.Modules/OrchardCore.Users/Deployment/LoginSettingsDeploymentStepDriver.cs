using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Users.Deployment
{
    public class LoginSettingsDeploymentStepDriver : DisplayDriver<DeploymentStep, LoginSettingsDeploymentStep>
    {
        public override IDisplayResult Display(LoginSettingsDeploymentStep step)
        {
            return
                Combine(
                    View("LoginSettingsDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("LoginSettingsDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(LoginSettingsDeploymentStep step)
        {
            return View("LoginSettingsDeploymentStep_Fields_Edit", step).Location("Content");
        }
    }
}
