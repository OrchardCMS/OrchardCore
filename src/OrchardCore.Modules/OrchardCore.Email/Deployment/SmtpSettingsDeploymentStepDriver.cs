using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Email.Deployment
{
    public class SmtpSettingsDeploymentStepDriver : DisplayDriver<DeploymentStep, SmtpSettingsDeploymentStep>
    {
        public override IDisplayResult Display(SmtpSettingsDeploymentStep step)
        {
            return
                Combine(
                    View("SmtpSettingsDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("SmtpSettingsDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(SmtpSettingsDeploymentStep step)
        {
            return View("SmtpSettingsDeploymentStep_Fields_Edit", step).Location("Content");
        }
    }
}
