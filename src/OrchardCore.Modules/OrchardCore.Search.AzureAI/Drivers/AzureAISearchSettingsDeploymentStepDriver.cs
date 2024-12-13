using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.AzureAI.Deployment;

namespace OrchardCore.Search.AzureAI.Drivers;

public sealed class AzureAISearchSettingsDeploymentStepDriver : DisplayDriver<DeploymentStep, AzureAISearchSettingsDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(AzureAISearchSettingsDeploymentStep step, BuildDisplayContext context)
        => CombineAsync(
            View("AzureAISearchSettingsDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
            View("AzureAISearchSettingsDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
        );

    public override IDisplayResult Edit(AzureAISearchSettingsDeploymentStep step, BuildEditorContext context)
        => View("AzureAISearchSettingsDeploymentStep_Fields_Edit", step).Location("Content");
}
