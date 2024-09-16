using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Search.Lucene.Deployment;

public sealed class LuceneSettingsDeploymentStepDriver : DisplayDriver<DeploymentStep, LuceneSettingsDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(LuceneSettingsDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("LuceneSettingsDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                View("LuceneSettingsDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(LuceneSettingsDeploymentStep step, BuildEditorContext context)
    {
        return View("LuceneSettingsDeploymentStep_Fields_Edit", step).Location("Content");
    }
}
