using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.Elasticsearch.Core.Deployment;

namespace OrchardCore.Search.Elasticsearch.Drivers;

public sealed class ElasticSettingsDeploymentStepDriver : DisplayDriver<DeploymentStep, ElasticSettingsDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(ElasticSettingsDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("ElasticSettingsDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                View("ElasticSettingsDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(ElasticSettingsDeploymentStep step, BuildEditorContext context)
    {
        return View("ElasticSettingsDeploymentStep_Fields_Edit", step).Location("Content");
    }
}
