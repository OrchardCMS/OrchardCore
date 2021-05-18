using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Google.Analytics.Deployment
{
    public class GoogleAnalyticsDeploymentStepDriver : DisplayDriver<DeploymentStep, GoogleAnalyticsDeploymentStep>
    {
        public override IDisplayResult Display(GoogleAnalyticsDeploymentStep step)
        {
            return
                Combine(
                    View("GoogleAnalyticsDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("GoogleAnalyticsDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(GoogleAnalyticsDeploymentStep step)
        {
            return View("GoogleAnalyticsDeploymentStep_Edit", step).Location("Content");
        }
    }
}
