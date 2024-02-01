using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Search.Lucene.Deployment
{
    public class LuceneSettingsDeploymentStepDriver : DisplayDriver<DeploymentStep, LuceneSettingsDeploymentStep>
    {
        public override IDisplayResult Display(LuceneSettingsDeploymentStep step)
        {
            return
                Combine(
                    View("LuceneSettingsDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("LuceneSettingsDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(LuceneSettingsDeploymentStep step)
        {
            return View("LuceneSettingsDeploymentStep_Fields_Edit", step).Location("Content");
        }
    }
}
