using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Search.Deployment
{
    public class SearchSettingsDeploymentStepDriver : DisplayDriver<DeploymentStep, SearchSettingsDeploymentStep>
    {
        public override IDisplayResult Display(SearchSettingsDeploymentStep step)
        {
            return
                Combine(
                    View("SearchSettingsDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("SearchSettingsDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(SearchSettingsDeploymentStep step)
        {
            return View("SearchSettingsDeploymentStep_Fields_Edit", step).Location("Content");
        }
    }
}
