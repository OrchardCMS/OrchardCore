using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.AzureAI.Deployment;

namespace OrchardCore.Search.AzureAI.Drivers;

public class AzureAISearchSettingsDeploymentStepDriver : DisplayDriver<DeploymentStep, AzureAISearchSettingsDeploymentStep>
{
    public override IDisplayResult Display(AzureAISearchSettingsDeploymentStep step)
        => Combine(
            View("AzureAISearchSettingsDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
            View("AzureAISearchSettingsDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
        );

    public override IDisplayResult Edit(AzureAISearchSettingsDeploymentStep step)
    {
        return View("AzureAISearchSettingsDeploymentStep_Fields_Edit", step).Location("Content");
    }
}
