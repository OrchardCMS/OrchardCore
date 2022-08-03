using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Search.Elasticsearch.Deployment
{
    public class ElasticsearchSettingsDeploymentStepDriver : DisplayDriver<DeploymentStep, ElasticsearchSettingsDeploymentStep>
    {
        public override IDisplayResult Display(ElasticsearchSettingsDeploymentStep step)
        {
            return
                Combine(
                    View("ElasticsearchSettingsDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("ElasticsearchSettingsDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(ElasticsearchSettingsDeploymentStep step)
        {
            return View("ElasticsearchSettingsDeploymentStep_Fields_Edit", step).Location("Content");
        }
    }
}
